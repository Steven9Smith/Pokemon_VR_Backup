using System;
using System.IO;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Data Serializer Interface
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serialize a data object and return the data as object.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <returns>The serialized data as an object</returns>
        object Serialize(object data);
        
        /// <summary>
        /// Serialize a data object and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data object to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <returns>The serialized data as an string</returns>
        string Serialize(object data, bool encrypted = false);

        /// <summary>
        /// Serialize the data of a generic type T and write the data through a stream. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="stream">The stream through which the serialization is supposed to be written</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        void Serialize<T>(T data, Stream stream, bool encrypted = false);


        /// <summary>
        /// Serialize the data of a generic type T and return the data as string. Can use encryption.
        /// </summary>
        /// <param name="data">The data as type T to serialize</param>
        /// <param name="encrypted">Is the serialization encrypted</param>
        /// <typeparam name="T">The type of the data to be serialized</typeparam>
        /// <returns>The serialized data as a string</returns>
        string Serialize<T>(T data, bool encrypted = false);
        
        
        /// <summary>
        /// Deserialize the data from a stream and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="stream">The stream to deserialize</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        T Deserialize<T>(Stream stream, bool encrypted = false);


        /// <summary>
        /// Deserialize the data from a string as the type T and return it as an object. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="t">The type of the data to be deserialized</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <returns>The deserialized data as an object</returns>
        object Deserialize(string data, Type t, bool encrypted = false);

        /// <summary>
        /// Deserialize the data from a byte array and return it as the type T. Can use encryption. 
        /// </summary>
        /// <param name="data">The data to deserialize as a byte array</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        T Deserialize<T>(byte[] data, bool encrypted = false);

        /// <summary>
        /// Deserialize the data from a string and return it as the type T. Can use encryption.
        /// </summary>
        /// <param name="data">The data to deserialize as a string</param>
        /// <param name="encrypted">Is the deserialization encrypted</param>
        /// <typeparam name="T">The type of the data to be deserialized</typeparam>
        /// <returns>The deserialized data as type T</returns>
        T Deserialize<T>(string data, bool encrypted = false);
    }
}