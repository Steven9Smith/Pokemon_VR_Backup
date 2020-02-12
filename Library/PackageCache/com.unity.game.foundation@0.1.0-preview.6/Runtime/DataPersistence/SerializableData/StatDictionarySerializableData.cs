using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a StatDictionary.
    /// </summary>
    [Serializable]
    public class StatDictionarySerializableData
    {
        /// <summary>
        /// The type of value of a StatDictionary
        /// </summary>
        public enum StatType
        {
            Int,
            Float
        }
        
        [SerializeField] StatType m_StatType;
        [SerializeField] StatSerializableData[] m_Stats = null;

        /// <summary>
        /// The type of value of the StatDictionary
        /// </summary>
        public StatType statType
        {
            get { return m_StatType; }
        }

        /// <summary>
        /// Array of all serialized stats of the StatDictionary
        /// </summary>
        public StatSerializableData[] stats
        {
            get { return m_Stats; }
        }

        /// <summary>
        /// Basic constructor that takes in the type of the StatDictionary and an array of all serializaed stats
        /// </summary>
        /// <param name="type">The type of value of the StatDictionary</param>
        /// <param name="stats">Array of all serialized stats of the StatDictionary</param>
        public StatDictionarySerializableData(StatType type, StatSerializableData[] stats)
        {
            m_StatType = type;
            m_Stats = stats;
        }
        
        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public StatDictionarySerializableData()
        {
        }
    }
}