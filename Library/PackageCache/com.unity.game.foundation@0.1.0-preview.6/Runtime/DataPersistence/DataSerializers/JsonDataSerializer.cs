using System;
using System.IO;

#if GAMEFOUNDATION_DATAVAULT
using UnityEngine.GameFoundation.DataSecurity;
#endif

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// DataSerializer to serialize to Json and deserialize from Json
    /// </summary>
    public sealed class JsonDataSerializer : DataSerializerBase
    {
        #if GAMEFOUNDATION_DATAVAULT
        private readonly IEncryption _dataEncryption;
        #endif        

        /// <summary>
        /// Default constructor of the Json DataSerialzier
        /// </summary>
        public JsonDataSerializer()
        {
            isBinarySerializer = false;
            serializerName = "Json";
            
            #if GAMEFOUNDATION_DATAVAULT
            _dataEncryption = EncryptionFactory.Produce();
            #endif
        }
        
        /// <summary>
        /// Json serialization method that serialize a data object and return the data as object.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <returns>The serialized data as an object</returns>
        public override object Serialize(object data)
        {
            return JsonUtility.ToJson(data);
        }
        
        /// <summary>
        /// Json serialization method that serialize a data object and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <returns>The serialized data as an string</returns>
        public override string Serialize(object data, bool encrypted = false)
        {
            var json = JsonUtility.ToJson(data);
            
            #if GAMEFOUNDATION_DATAVAULT
                if (encrypted) json = _dataEncryption.Encrypt(json);   
            #endif
            
            return json;
        }

        /// <summary>
        /// Json serialization method that serialize the data of a generic type T and write the data through a stream. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="stream">The stream through which the serialization is supposed to be written</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        public override void Serialize<T>(T data, Stream stream, bool encrypted = false)
        {
            StreamWriter sw = new StreamWriter ( stream );
            var json = Serialize(data, encrypted);
            
            sw.Write(json);
            sw.Dispose ();
        }
        
        /// <summary>
        /// Json serialization method that serialize the data of a generic type T and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        /// <returns>The serialized data as an string</returns>
        public override string Serialize<T>(T data, bool encrypted = false)
        {
            var json = JsonUtility.ToJson(data);
            
            #if GAMEFOUNDATION_DATAVAULT
                if (encrypted) json = _dataEncryption.Encrypt(json);
            #endif
            
            return json;
        }
        
        /// <summary>
        /// Json deserialization method that deserialize the data from a stream and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="stream">The stream to deserialize</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public override T Deserialize<T>(Stream stream, bool encrypted = false)
        {
            T result = default(T);
                        
            StreamReader sr = new StreamReader ( stream );
            result = JsonUtility.FromJson<T> ( sr.ReadToEnd () );
            sr.Dispose ();
            return result;
            
        }
        
        /// <summary>
        /// Json deserialzation method that deserialize the data from a string as the type T and return it as an object. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="t">The type of the data to be deserialized</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <returns>The deserialized data as an object</returns>
        public override object Deserialize(string data, Type t, bool encrypted = false)
        {
            #if GAMEFOUNDATION_DATAVAULT
                return JsonUtility.FromJson<object>(encrypted ? _dataEncryption.Decrypt(data) : data);
            #else
                return data;
            #endif
        }
        
        /// <summary>
        /// This method is not implemented yet.
        /// Json deserialization method that deserialize the data from a byte array and return it as the type T. Can use encryption. 
        /// </summary>
        /// <param name="data">The data to deserialize as a byte array</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public override T Deserialize<T>(byte[] data, bool encrypted = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Json deserialization method that deserialize the data from a string and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public override T Deserialize<T>(string data, bool encrypted = false)
        {
            #if GAMEFOUNDATION_DATAVAULT
                return JsonUtility.FromJson<T>(encrypted ? _dataEncryption.Decrypt(data) : data);
            #else
                return JsonUtility.FromJson<T>(data);
            #endif
        }
    }

}