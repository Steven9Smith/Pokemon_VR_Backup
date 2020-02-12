using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of GameItemLookup.
    /// </summary>
    [Serializable]
    public class GameItemLookupSerializableData : ISerializableData
    {
        [SerializeField] int m_LastGuidUsed;
        
        /// <summary>
        /// The last guid used by the GameItemLookup class
        /// </summary>
        public int lastGuidUsed
        {
            get { return m_LastGuidUsed; }
        }

        /// <summary>
        /// Basic constructor that takes in the version of the persistence layer and the index of the last GUID used by the GameItemLookup.
        /// </summary>
        /// <param name="lastGuidUsed"></param>
        public GameItemLookupSerializableData(int lastGuidUsed)
        {
            m_LastGuidUsed = lastGuidUsed;
        }
        
        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public GameItemLookupSerializableData()
        {
        }
    }
}