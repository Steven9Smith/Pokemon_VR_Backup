using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory.
    /// </summary>
    [Serializable]
    public class InventorySerializableData
    {
        [SerializeField] string m_DefinitionId = null;
        [SerializeField] string m_InventoryId = null;
        [SerializeField] InventoryItemSerializableData[] m_Items = null;
        [SerializeField] int m_GameItemLookupId;
        
        /// <summary>
        /// The definition Id of the inventory
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }
        
        /// <summary>
        /// The inventory id of the inventory
        /// </summary>
        public string inventoryId
        {
            get { return m_InventoryId; }
        }

        /// <summary>
        /// The items this inventory contains
        /// </summary>
        public InventoryItemSerializableData[] items
        {
            get { return m_Items; }
        }

        /// <summary>
        /// The GameItemId of the item use by GameItemLookup
        /// </summary>
        public int gameItemLookupId
        {
            get { return m_GameItemLookupId; }
        }
        
        /// <summary>
        /// Basic constructor that takes in an inventory definition id and an array of InventoryItemData of all inventory items contained in the inventory.
        /// </summary>
        /// <param name="definitionId">The definition id of the inventory</param>
        /// <param name="items">The inventory items contained in the inventory</param>
        /// <param name="gameItemLookupId">The GameItemId of the item use by GameItemLookup</param>
        public InventorySerializableData(string definitionId, string inventoryId, InventoryItemSerializableData[] items, int gameItemLookupId)
        {
            m_DefinitionId = definitionId;
            m_InventoryId = inventoryId;
            m_Items = items;
            m_GameItemLookupId = gameItemLookupId;
        }

        /// <summary>
        /// Default constructor for serialization purpose
        /// </summary>
        public InventorySerializableData()
        {
        }
    }
}