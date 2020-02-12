using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    public interface IDataPersistence
    {
        /// <summary>
        /// Asynchronously loads data from the persistence layer. Deserialized data, when loaded, are passed as second argument
        /// of the given onFinish Action. The generic param will be provided by the serializer underneath and need to implement
        /// ISerializableData
        /// </summary>
        /// <param name="identifier">Identifier of the persistence entry (filename, url, ...)</param>
        /// <param name="onLoadCompleted">Called when the loading is completed with success</param>
        /// <param name="onLoadFailed">Called when the loading failed</param>
        void Load<T>(string identifier, Action<ISerializableData> onLoadCompleted = null, Action onLoadFailed = null) where T : ISerializableData;

        /// <summary>
        /// Asynchronously saves data onto the persistence layer. When the async save operation is done, onFinish Action get called.
        /// </summary>
        /// <param name="identifier">Identifier of the persistence entry (filename, url, ...)</param>
        /// <param name="content">Data to persist (need to be serializable)</param>
        /// <param name="onSaveCompleted">Called when the saving is completed with success</param>
        /// <param name="onSaveFailed">Called when the loading failed</param>
        void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action onSaveFailed = null);
    }
}
