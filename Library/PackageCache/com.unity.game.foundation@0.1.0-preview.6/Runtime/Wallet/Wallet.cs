using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Static helper class for dealing with the Wallet inventory.  
    /// Many methods are simply shortcuts for 'InventoryManager.wallet.xxx'.
    /// </summary>
    public static class Wallet
    {
        /// <summary>
        /// Adds a new entry of the specified InventoryItemDefinition by Id. Returns the new or existing InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="itemdefinitionId">The Id of the InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The new InventoryItem that was added, or null if Id is invalid.</returns>
        public static InventoryItem AddItem(string itemdefinitionId, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(itemdefinitionId, quantity);
        }

        /// <summary>
        /// Adds more of the specified InventoryItemDefinition by Hash. Returns the new (or existing) InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing reference when to the InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The InventoryItem that was added.</returns>
        public static InventoryItem AddItem(int itemDefinitionHash, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(itemDefinitionHash, quantity);
        }

        /// <summary>
        /// Adds more of the specified InventoryItemDefinition. Returns the new (or existing) InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing reference when to the InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The InventoryItem that was added.</returns>
        public static InventoryItem AddItem(InventoryItemDefinition itemDefinition, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(itemDefinition, quantity);
        }

        /// <summary>
        /// The InventoryDefinition of this Wallet. Determines the default InventoryItems and quantities.
        /// </summary>
        /// <returns>The InventoryDefinition of this Wallet.</returns>
        public static InventoryDefinition definition
        {
            get { return InventoryManager.wallet.collectionDefinition; }
        }

        /// <summary>
        /// Helper property for easily accessing the Id of the Wallet's InventoryDefinition.
        /// </summary>
        /// <returns>The Id of the Wallet's InventoryDefinition</returns>
        public static string definitionId
        {
            get { return InventoryManager.wallet.collectionDefinitionId; }
        }

        /// <summary>
        /// Helper property for easily accessing the Wallet's InventoryDefinition Hash.
        /// </summary>
        /// <returns>The Wallet's InventoryDefinition Hash.</returns>
        public static int definitionHash
        {
            get { return InventoryManager.wallet.collectionDefinitionHash; }
        }

        /// <summary>
        /// Returns an array of all items within the wallet.
        /// </summary>
        /// <returns>An array of all items within the wallet.</returns>
        public static InventoryItem[] GetItems()
        {
            return InventoryManager.wallet.GetItems();
        }

        /// <summary>
        /// Fills the given list with all items found in the wallet.
        /// </summary>
        /// <param name="inventoryItems">The list to fill up.</param>
        public static void GetItems(List<InventoryItem> inventoryItems)
        {
            InventoryManager.wallet.GetItems(inventoryItems);
        }

        /// <summary>
        /// Gets an InventoryItem by InventoryItemDefinition reference if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the InventoryItem to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItem(string itemDefinitionId)
        {
            return InventoryManager.wallet.GetItem(itemDefinitionId);
        }

        /// <summary>
        /// Gets an InventoryItem by InventoryItemDefinition Hash if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="itemDefinitionHash">The InventoryItemDefinition Hash of the InventoryItem to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItem(int itemDefinitionHash)
        {
            return InventoryManager.wallet.GetItem(itemDefinitionHash);
        }

        /// <summary>
        /// Returns all an array of items in the wallet with the given category id.
        /// </summary>
        /// <param name="categoryId">The category id to check.</param>
        /// <returns>An array of items in the wallet with the given category id.</returns>
        public static InventoryItem[] GetItemsByCategory(string categoryId)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryId);
        }

        /// <summary>
        /// Fills the given list with items with the given category id.
        /// </summary>
        /// <param name="categoryId">The category id to check.</param>
        /// <param name="inventoryItems">The list to fill up.</param>
        public static void GetItemsByCategory(string categoryId, List<InventoryItem> inventoryItems)
        {
            InventoryManager.wallet.GetItemsByCategory(categoryId, inventoryItems);
        }

        /// <summary>
        /// This will return all InventoryItems that have the given Category through an array.
        /// </summary>
        /// <param name="categoryDefinition">The CategoryDefinition we are checking for.</param>
        /// <returns>An array of the InventoryItems that have the given Category.</returns>
        public static InventoryItem[] GetItemsByCategory(CategoryDefinition categoryDefinition)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryDefinition);
        }

        /// <summary>
        /// Fills the given list with items with the given category.
        /// </summary>
        /// <param name="categoryDefinition">The category to check.</param>
        /// <param name="inventoryItems">The list to fill up.</param>
        public static void GetItemsByCategory(CategoryDefinition categoryDefinition, List<InventoryItem> inventoryItems)
        {
            InventoryManager.wallet.GetItemsByCategory(categoryDefinition, inventoryItems);
        }

        /// <summary>
        /// This will return an array of all InventoryItems that have the given Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to check.</param>
        /// <returns>An array of the InventoryItems that have the given Category.</returns>
        public static InventoryItem[] GetItemsByCategory(int categoryHash)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryHash);
        }

        /// <summary>
        /// Fills the given list with items with the given category hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to check.</param>
        /// <param name="inventoryItems">The list to fill up.</param>
        public static void GetItemsByCategory(int categoryHash, List<InventoryItem> inventoryItems)
        {
            InventoryManager.wallet.GetItemsByCategory(categoryHash, inventoryItems);
        }

        /// <summary>
        /// Removes an entry of the specified InventoryItem by InventoryItemDefintion id.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the InventoryItemDefinition we are removing.</param>
        public static void RemoveItem(string itemDefinitionId)
        {
            InventoryManager.wallet.RemoveItem(itemDefinitionId);
        }

        /// <summary>
        /// Removes an entry of the specified InventoryItemefinition by Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition to remove.</param>
        public static void RemoveItem(int itemDefinitionHash)
        {
            InventoryManager.wallet.RemoveItem(itemDefinitionHash);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category by CategoryDefinition Id.
        /// </summary>
        /// <param name="categoryId">The Id of the CategoryDefinition we are checking for.</param>
        public static void RemoveItemsByCategory(string categoryId)
        {
            InventoryManager.wallet.RemoveItemsByCategory(categoryId);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category.
        /// </summary>
        /// <param name="category">The CategoryDefinition to remove.</param>
        public static void RemoveItemsByCategory(CategoryDefinition category)
        {
            InventoryManager.wallet.RemoveItemsByCategory(category);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category by CategoryDefinition Hash.
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition to remove.</param>
        public static void RemoveItemsByCategory(int categoryHash)
        {
            InventoryManager.wallet.RemoveItemsByCategory(categoryHash);
        }

        /// <summary>
        /// Removes all InventoryItems from the Wallet.
        /// </summary>
        public static void RemoveAll()
        {
            InventoryManager.wallet.RemoveAll();
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the InventoryItemDefinition to find.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(string itemDefinitionId)
        {
            return InventoryManager.wallet.ContainsItem(itemDefinitionId);
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet for specified InventoryItemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition to find.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(InventoryItemDefinition itemDefinition)
        {
            return InventoryManager.wallet.ContainsItem(itemDefinition);
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(int itemDefinitionHash)
        {
            return InventoryManager.wallet.ContainsItem(itemDefinitionHash);
        }

        /// <summary>
        /// Resets the contents of the Wallet.
        /// </summary>
        public static void Reset()
        {
            InventoryManager.wallet.Reset();
        }
        
        /// <summary>
        /// Gets the Quantity of an InventoryItem by its InventoryItemDefinition id
        /// </summary>
        /// <param name="inventoryItemDefinitionId">InventoryItemDefinition id of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="ArgumentNullException">The parameter id is null or empty</exception>
        public static int GetQuantity(string inventoryItemDefinitionId)
        {
            return InventoryManager.wallet.GetQuantity(inventoryItemDefinitionId);
        }

        /// <summary>
        /// Gets the Quantity of an InventoryItem by its InventoryItemDefinition hash
        /// </summary>
        /// <param name="inventoryItemDefinitionHash">InventoryItemDefinition hash of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="KeyNotFoundException">The hash provided doesn't correspond to a valid item</exception>
        public static int GetQuantity(int inventoryItemDefinitionHash)
        {
            return InventoryManager.wallet.GetQuantity(inventoryItemDefinitionHash);
        }

        /// <summary>
        /// Gets the Quantity of an InventoryItem by its reference
        /// </summary>
        /// <param name="inventoryItem">InventoryItem reference of the item's quantity to get</param>
        /// <returns>Quantity of the requested Inventory Item.</returns>
        /// <exception cref="ArgumentNullException">The parameter id is null</exception>
        public static int GetQuantity(InventoryItem inventoryItem)
        {
            return InventoryManager.wallet.GetQuantity(inventoryItem);
        }

        /// <summary>
        /// Sets the quantity of an InventoryItem by InventoryItemDefinition Id.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the InventoryItemDefinition we are checking for.</param>
        /// <param name="quantity">The new value we are setting.</param>
        public static void SetQuantity(string itemDefinitionId, int quantity)
        {
            InventoryManager.wallet.SetQuantity(itemDefinitionId, quantity);
        }

        /// <summary>
        /// Sets the quantity of the InventoryItem within this Wallet of the specified InventoryItemDefinition by Hash
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the InventoryItemDefinition to set quantity for.</param>
        /// <param name="quantity">The new quantity value to set.</param>
        public static void SetQuantity(int itemDefinitionHash, int quantity)
        {
            InventoryManager.wallet.SetQuantity(itemDefinitionHash, quantity);
        }

        /// <summary>	
        /// Sets the quantity of an InventoryItem.
        /// </summary>	
        /// <param name="item">The InventoryItem to set quantity for.</param>	
        /// <param name="quantity">The new quantity value to set.</param>	
        public static void SetQuantity(InventoryItem item, int quantity)
        {
            InventoryManager.wallet.SetQuantity(item.id, quantity);
        }
        
        /// <summary>
        /// This is a callback that will be invoked whenever an InventoryItem is added to the Wallet.
        /// </summary>
        /// <returns>A CollectionItemEvent fired whenever an InventoryItem is added to the Wallet.</returns>
        public static Inventory.BaseCollectionItemEvent onItemAdded
        {
            get { return InventoryManager.wallet.onItemAdded; }
            set { InventoryManager.wallet.onItemAdded = value; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an InventoryItem is removed from the Wallet.
        /// </summary>
        /// <returns>A CollectionItemEvent fired whenever an InventoryItem is removed from the Wallet.</returns>
        public static Inventory.BaseCollectionItemEvent onItemRemoved
        {
            get { return InventoryManager.wallet.onItemRemoved; }
            set { InventoryManager.wallet.onItemRemoved = value; }
        }

        /// <summary>
        /// Callback for when an InventoryItem quantity has changed.
        /// </summary>
        /// <returns>A CollectionItemEvent fired when an InventoryItem quantity has changed.</returns>
        public static Inventory.BaseCollectionItemEvent onItemQuantityChanged
        {
            get { return InventoryManager.wallet.onItemQuantityChanged; }
            set { InventoryManager.wallet.onItemQuantityChanged = value; }
        }

        /// <summary>
        /// Callback for when an InventoryItem quantity has gone above its minimum.
        /// </summary>
        /// <returns>The CollectionItemEvent fired when an InventoryItem quantity has gone above its minimum.</returns>
        public static Inventory.BaseCollectionItemEvent onItemQuantityOverflow
        {
            get { return InventoryManager.wallet.onItemQuantityOverflow; }
            set { InventoryManager.wallet.onItemQuantityOverflow = value; }
        }
    }
}
