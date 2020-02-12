namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of Game Foundation.
    /// </summary>
    [System.Serializable]
    public class GameFoundationSerializableData : ISerializableData
    {
        [SerializeField] InventoryManagerSerializableData m_InventoryManagerData;
        [SerializeField] StatManagerSerializableData m_StatManagerData;
        [SerializeField] GameItemLookupSerializableData m_GameItemLookupData;
        
        [SerializeField] int m_Version = 0;

        /// <summary>
        /// The data of InventoryManager to be persisted.
        /// </summary>
        public InventoryManagerSerializableData inventoryManagerData
        {
            get { return m_InventoryManagerData; }
        }

        /// <summary>
        /// The data of StatManager to be persisted.
        /// </summary>
        public StatManagerSerializableData statManagerData
        {
            get { return m_StatManagerData; }
        }

        /// <summary>
        /// The data of GameItemLookup to be persisted.
        /// </summary>
        public GameItemLookupSerializableData gameItemLookupData
        {
            get { return m_GameItemLookupData; }
        }
        
        /// <summary>
        /// The version of of the save schematic
        /// </summary>
        public int version
        {
            get { return m_Version; }
        }
        
        /// <summary>
        /// Basic constructor that take the stat manager, the inventory manager data and the gameItemLookup data.
        /// </summary>
        /// <param name="version">version">The version of the save schematic</param>
        /// <param name="statData">The serializable data of the StatManager</param>
        /// <param name="inventoryData">The serializable data of the InventoryManager</param>
        /// <param name="lookupData">The serializable data of GameItemLookup</param>
        public GameFoundationSerializableData(int version, StatManagerSerializableData statData, InventoryManagerSerializableData inventoryData, GameItemLookupSerializableData lookupData)
        {
            m_Version = version;
            
            m_StatManagerData = statData;
            m_InventoryManagerData = inventoryData;
            m_GameItemLookupData = lookupData;
        }

        /// <summary>
        /// Default constructor for serialization purpose
        /// </summary>
        public GameFoundationSerializableData()
        {
        }
    }
}