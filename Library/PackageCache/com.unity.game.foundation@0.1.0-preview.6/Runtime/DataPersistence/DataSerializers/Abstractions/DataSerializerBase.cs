using System;
using System.IO;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Base abstraction class of the IDataSerializer interface.
    /// </summary>
    public abstract class DataSerializerBase : IDataSerializer
    {
        bool m_IsBinarySerializer = true;
        string m_SerializerName;

        /// <summary>
        /// Is the serializer binary. Default is set to true.
        /// </summary>
        public bool isBinarySerializer
        {
            get { return m_IsBinarySerializer; }
            protected set { m_IsBinarySerializer = value; }
        }

        /// <summary>
        /// The serializer name.
        /// </summary>
        public string serializerName
        {
            get { return m_SerializerName; }
            protected set { m_SerializerName = value; }
        }
        
        /// <summary>
        /// Get the name of the serializer.
        /// </summary>
        /// <returns>The name of the serializer</returns>
        public string Name()
        {
            return m_SerializerName;
        }

        /// <summary>
        /// Return a bool that indicate if the serializer is binary type.
        /// </summary>
        /// <returns>Whether or not the serializer is binary type</returns>
        public bool IsBinary()
        {
            return m_IsBinarySerializer;
        }

        /// <summary>
        /// Public abstraction of the serialize method that serialize a data object and return the data as object.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <returns>The serialized data as an object</returns>
        public abstract object Serialize(object data);

        /// <summary>
        /// Public abstraction of the serialize method that serialize a data object and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <returns>The serialized data as an string</returns>
        public abstract string Serialize(object data, bool encrypted = false);
        
        /// <summary>
        /// Public abstraction of the serialize method that serialize the data of a generic type T and write the data through a stream. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="stream">The stream through which the serialization is supposed to be written</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        public abstract void Serialize<T>(T data, Stream stream, bool encrypted = false);

        /// <summary>
        /// Public abstraction of the serialize method that serialize the data of a generic type T and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        /// <returns>The serialized data as an string</returns>
        public abstract string Serialize<T>(T data, bool encrypted = false);

        /// <summary>
        /// Public abstraction of the deserialize method that deserialize the data from a stream and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="stream">The stream to deserialize</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public abstract T Deserialize<T>(Stream stream, bool encrypted = false);

        /// <summary>
        /// Public abstraction of the deserialize method that deserialize the data from a string as the type T and return it as an object. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="t">The type of the data to be deserialized</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <returns>The deserialized data as an object</returns>
        public abstract object Deserialize(string data, Type t, bool encrypted = false);

        /// <summary>
        /// Public abstraction of the deserialize method that deserialize the data from a byte array and return it as the type T. Can use encryption. 
        /// </summary>
        /// <param name="data">The data to deserialize as a byte array</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public abstract T Deserialize<T>(byte[] data, bool encrypted = false);
        
        /// <summary>
        /// Public abstraction of the deserialize method that deserialize the data from a string and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        public abstract T Deserialize<T>(string data, bool encrypted = false);
       
    }
}