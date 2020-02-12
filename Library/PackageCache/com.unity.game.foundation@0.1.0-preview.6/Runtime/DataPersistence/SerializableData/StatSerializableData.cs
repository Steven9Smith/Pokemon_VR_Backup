using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a Stat.
    /// </summary>
    [Serializable]
    public class StatSerializableData
    {
        [SerializeField] long m_StatDictionaryId;
        [SerializeField] StatItemSerializableData m_StatItem;

        /// <summary>
        /// The id of the stat in the dictionary.
        /// </summary>
        public long statDictionaryId
        {
            get { return m_StatDictionaryId; }
        }

        /// <summary>
        /// The serialized data of the stat.
        /// </summary>
        public StatItemSerializableData statItem
        {
            get { return m_StatItem; }
        }

        /// <summary>
        /// Basic constructor that take in the id of the item in the dictionary and the serialized data of this stat.
        /// </summary>
        public StatSerializableData(long statDictionaryId, StatItemSerializableData statItem)
        {
            m_StatDictionaryId = statDictionaryId;
            m_StatItem = statItem;
        }
        
        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public StatSerializableData()
        {
        }
    }
}