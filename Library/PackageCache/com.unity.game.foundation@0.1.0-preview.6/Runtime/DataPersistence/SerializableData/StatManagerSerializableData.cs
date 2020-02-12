using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the StatManager.
    /// </summary>
    [Serializable]
    public class StatManagerSerializableData : ISerializableData
    {
        [SerializeField] StatDictionarySerializableData[] m_StatDictionaries = null;
        
        /// <summary>
        /// Return the data of all runtime stat dictionaries
        /// </summary>
        public StatDictionarySerializableData[] statDictionaries
        {
            get { return m_StatDictionaries; }
        }
        
        /// <summary>
        /// Basic constructor that takes in an array of StatDictionarySerializableData of all runtime stat dictionaries.
        /// </summary>
        /// <param name="statDictionaries">The StatDictionarySerializableData array the StatManagerSerializableData is based of.</param>
        public StatManagerSerializableData(StatDictionarySerializableData[] statDictionaries)
        {
            m_StatDictionaries = statDictionaries;
        }

        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public StatManagerSerializableData()
        {
        }
    }
}