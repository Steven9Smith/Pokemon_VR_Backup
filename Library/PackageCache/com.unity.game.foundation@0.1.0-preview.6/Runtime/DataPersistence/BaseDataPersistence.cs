using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Base persistence class derived from IDataPersistence
    /// </summary>
    public abstract class BaseDataPersistence : IDataPersistence
    {
        public static readonly int k_SaveVersion = 1;

        IDataSerializer m_Serializer;

        /// <summary>
        /// The serialization layer used by the processes of this persistence.
        /// </summary>
        protected IDataSerializer serializer
        {
            get { return m_Serializer; }
        }

        public BaseDataPersistence(IDataSerializer serializer)
        {
            m_Serializer = serializer;
        }

        /// <inheritdoc />
        public abstract void Load<T>(string identifier, Action<ISerializableData> onLoadCompleted = null, Action onLoadFailed = null) where T : ISerializableData;
        
        /// <inheritdoc />
        public abstract void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action onSaveFailed = null);
    }
}
