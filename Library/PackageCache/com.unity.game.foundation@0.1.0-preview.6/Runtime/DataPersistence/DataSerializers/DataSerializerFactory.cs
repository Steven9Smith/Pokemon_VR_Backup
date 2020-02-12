using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Data Serializer factory class
    /// </summary>
    public static class DataSerializerFactory
    {
        /// <summary>
        /// Produces an instance of a specified serializer type 
        /// </summary>
        /// <param name="type">Serializer type to produce</param>
        /// <returns>Specified serializer instance</returns>
        /// <exception cref="NotImplementedException">Exception thrown when given DataSerializerType passed to method is a type that is not supported</exception>
        public static IDataSerializer Produce(DataSerializerType type)
        {
            switch(type)
            {
                case DataSerializerType.Json:
                    return new JsonDataSerializer();
                default:
                    throw new NotImplementedException();
            }
        }
    }

}