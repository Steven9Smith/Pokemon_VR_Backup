using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory item.
    /// </summary>
    [Serializable]
    public class InventoryItemSerializableData
    {
        [SerializeField] string m_DefinitionId = null;
        [SerializeField] int m_Quantity = 0;
        [SerializeField] int m_GameItemLookupId;

        /// <summary>
        /// The definition Id of the inventory item
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }

        /// <summary>
        /// The quantity of the inventory item in the inventory.
        /// </summary>
        public int quantity
        {
            get { return m_Quantity; }
        }

        /// <summary>
        /// The GameItemId of the item use by GameItemLookup
        /// </summary>
        public int gameItemLookupId
        {
            get { return m_GameItemLookupId; }
        }
        
        /// <summary>
        /// Basic constructor that takes in the inventory item definition Id of the item and the quantity it have in this inventory.
        /// </summary>
        /// <param name="definitionId">The definition Id of the inventory item</param>
        /// <param name="quantity">The quantity of this item contained in the inventory</param>
        /// <param name="gameItemLookupId">The GameItemId of the item use by GameItemLookup</param>
        public InventoryItemSerializableData(string definitionId, int quantity, int gameItemLookupId)
        {
            m_DefinitionId = definitionId;
            m_Quantity = quantity;
            m_GameItemLookupId = gameItemLookupId;
        }

        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public InventoryItemSerializableData()
        {
        }
    }
}