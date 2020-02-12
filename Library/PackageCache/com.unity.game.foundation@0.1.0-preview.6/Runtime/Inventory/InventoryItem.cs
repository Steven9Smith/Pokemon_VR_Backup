using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// An InventoryItem that goes into an Inventory. InventoryItems should only exist inside Inventories. 
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItem : BaseItem<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// Basic constructor that takes in a reference to the InventoryItemDefinition that this is based on.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition this item is based on.</param>
        internal InventoryItem(InventoryItemDefinition itemDefinition, Inventory owner, int gameItemId = 0) : base(itemDefinition, owner, gameItemId)
        {
        }

        /// <summary>
        /// The Inventory that this InventoryItem belongs to.
        /// </summary>
        /// <returns>The Inventory that this InventoryItem belongs to.</returns>
        public Inventory inventory
        {
            get { return owner; }
        }

        /// <summary>
        /// Quantity contained in this Inventory for this InventoryItem.
        /// </summary>
        /// <returns>Quantity contained in this Inventory for this InventoryItem.</returns>
        public int quantity
        {
            get { return intValue; }
            set { SetQuantity(value); }
        }

        /// <summary>
        /// Sets the quantity of this InventoryItem.
        /// </summary>
        /// <param name="value">The new quantity for specified InventoryItemDefinition.</param>
        public void SetQuantity(int value)
        {
            if (inventory != null)
            {
                inventory.SetQuantity(id, value);
            }
            else
            {
                intValue = value;
            }
        }
    }
}
