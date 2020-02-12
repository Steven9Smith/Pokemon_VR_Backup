using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contains a runtime list of InventoryItems as well as details which effect this Inventory.
    /// </summary>
    /// <inheritdoc/>
    public class Inventory : BaseCollection<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// This is a reference to the InventoryManager's main Inventory.
        /// </summary>
        /// <returns>A reference to the InventoryManager's main Inventory.</returns>
        public static Inventory main
        {
            get { return InventoryManager.main; }
        }

        /// <summary>
        /// Constructor for an Inventory from the specified InventoryDefinition.
        /// </summary>
        /// <param name="inventoryDefinition">The InventoryDefinition this Inventory is based off of.</param>
        /// <param name="inventoryId">The Id this Inventory will use.</param>
        internal Inventory(InventoryDefinition inventoryDefinition, string inventoryId = null) 
            : this(inventoryDefinition, inventoryId, 0)
        {
        }
        
        internal Inventory(InventoryDefinition inventoryDefinition, string inventoryId, int gameItemId) 
            : base(inventoryDefinition, inventoryId, gameItemId)
        {
            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("Inventory can only be alphanumeric with optional dashes or underscores.");
            }
        }

        /// <summary>
        /// Sets the quantity of the specified InventoryItem by InventoryItemDefinition Id in this Inventory.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the InventoryItemDefinition to set quantity for.</param>
        /// <param name="quantity">The new quantity for specified InventoryItemDefinition.</param>
        public void SetQuantity(string itemDefinitionId, int quantity)
        {
            SetIntValue(itemDefinitionId, quantity);
        }

        /// <summary>
        /// Sets the quantity of the specified InventoryItem by InventoryItemDefinition Hash in this Inventory.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition to set.</param>
        /// <param name="quantity">The new quantity for the specified InventoryItemDefinition.</param>
        public void SetQuantity(int itemDefinitionHash, int quantity)
        {
            SetIntValue(itemDefinitionHash, quantity);
        }

        /// <summary>
        /// Sets the quantity of the InventoryItem instance within this Inventory.
        /// </summary>
        /// <param name="item">The InventoryItem to set quantity for in this Inventory.</param>
        /// <param name="quantity">The new value for quantity for this InventoryItem.</param>
        public void SetQuantity(InventoryItem item, int quantity)
        {
            SetIntValue(item.hash, quantity);
        }

        /// <summary>
        /// Gets the Quantity of an InventoryItem by its InventoryItemDefinition id
        /// </summary>
        /// <param name="inventoryItemDefinitionId">InventoryItemDefinition id of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="ArgumentNullException">The parameter id is null or empty</exception>
        public int GetQuantity(string inventoryItemDefinitionId)
        {
            if (string.IsNullOrEmpty(inventoryItemDefinitionId))
            {
                throw new ArgumentNullException(nameof(inventoryItemDefinitionId), "Given InventoryItemDefinition Id is null or empty.");
            }
            
            return GetQuantity(Tools.StringToHash(inventoryItemDefinitionId));
        }

        /// <summary>
        /// Gets the Quantity of an InventoryItem by its InventoryItemDefinition hash
        /// </summary>
        /// <param name="inventoryItemDefinitionHash">InventoryItemDefinition hash of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="KeyNotFoundException">The hash provided doesn't correspond to a valid item</exception>
        public int GetQuantity(int inventoryItemDefinitionHash)
        {
            InventoryItem inventoryItem = GetItem(inventoryItemDefinitionHash);
            if (inventoryItem == null)
            {
                throw new KeyNotFoundException("The hash provided isn't mapped to an item definition.");
            }

            return inventoryItem.quantity;
        }

        /// <summary>
        /// Gets the Quantity of an InventoryItem by its reference
        /// </summary>
        /// <param name="inventoryItem">InventoryItem reference of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="ArgumentNullException">The parameter id is null</exception>
        public int GetQuantity(InventoryItem inventoryItem)
        {
            if (inventoryItem == null)
            {
                throw new ArgumentNullException(nameof(inventoryItem), "Given InventoryItem is null.");
            }
            
            return GetQuantity(inventoryItem.hash);
        }

        protected override BaseItemDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem> GetItemDefinition(int itemDefinitionHash)
        {
            return InventoryManager.catalog == null ? null : InventoryManager.catalog.GetItemDefinition(itemDefinitionHash);
        }

        /// <summary>
        /// Adds a new entry of the specified InventoryItemDefinition by Hash. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the item didn't already exist in the Inventory and
        /// returns an existing reference when to the instance when the Item was already in the Inventory.
        /// Items added to the wallet Inventory must contain an attached CurrencyDetailDefinition.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition to set.</param>
        /// <param name="quantity">The new quantity for the specified InventoryItemDefinition.</param>
        /// <returns>The new or existing instance of the InventoryItem.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override InventoryItem AddItem(int itemDefinitionHash, int quantity = 1)
        {
            if (hash.Equals(InventoryManager.walletInventoryHash))
            {
                if (GetItemDefinition(itemDefinitionHash)?.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    throw new InvalidOperationException("ItemDefinition must have a CurrencyDetailDefinition DetailDefinition. Adding a non-currency item to the wallet Inventory is invalid.");
                }
            }
            return base.AddItem(itemDefinitionHash, quantity);
        }

        /// <summary>
        /// Returns a summary string for this Inventory.
        /// </summary>
        /// <returns>Summary string for this Inventory.</returns>
        public override string ToString()
        {
            return $"Inventory(Id: '{id}' DisplayName: '{displayName}' Definition: '{definition.id}' Count: {itemsInCollection?.Count}";
        }
    }
}
