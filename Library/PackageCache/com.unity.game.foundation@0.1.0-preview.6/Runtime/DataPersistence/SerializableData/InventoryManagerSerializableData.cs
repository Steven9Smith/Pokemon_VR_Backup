using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the InventoryManager.
    /// </summary>
    [Serializable]
    public class InventoryManagerSerializableData : ISerializableData
    {
        [SerializeField] InventorySerializableData[] m_Inventories = null;
        
        /// <summary>
        /// Return the data of all runtime inventories
        /// </summary>
        public InventorySerializableData[] inventories
        {
            get { return m_Inventories; }
        }

        /// <summary>
        /// Return the data of the Inventory using the specified definition id
        /// </summary>
        /// <param name="definitionId">The definition id of the Inventory we want.</param>
        /// <returns>The data of the Inventory with the requested Id.</returns>
        public InventorySerializableData GetInventory(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                return null;
            }
            
            foreach (var inventory in m_Inventories)
            {
                if (inventory.definitionId == definitionId)
                {
                    return inventory;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Basic constructor that takes in an array of InventoryData of all runtime inventories.
        /// </summary>
        /// <param name="inventories">The InventoryData array the RuntimeInventoryCatalogData is based of.</param>
        public InventoryManagerSerializableData(InventorySerializableData[] inventories)
        {
            m_Inventories = inventories;
        }

        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public InventoryManagerSerializableData()
        {
        }
    }
}