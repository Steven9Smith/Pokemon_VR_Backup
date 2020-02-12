using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manages all Inventories. Can subscribe to events relevant to Inventories and create and remove them here.
    /// InventoryManager also owns the main and wallet Inventories, as well as all other Inventories of InventoryItems.
    /// The InventoryManager can create Inventories from InventoryDefinitions or default Inventories as needed.
    /// </summary>
    public static class InventoryManager
    {
        private static readonly int k_MainInventoryHash = Tools.StringToHash(InventoryCatalog.k_MainInventoryDefinitionId);

        private static readonly int k_WalletInventoryHash = Tools.StringToHash(InventoryCatalog.k_WalletInventoryDefinitionId);

        /// <summary>
        /// This is the 'main' Inventory Id as a Hash for quick lookup and compares.
        /// </summary>
        /// <returns>The 'main' Inventory Id as a Hash for quick lookup and compares.</returns>
        internal static int mainInventoryHash { get; } = k_MainInventoryHash;

        /// <summary>
        /// This is a reference to the "main" Inventory for the InventoryManager.
        /// </summary>
        /// <returns>The "main" Inventory for the InventoryManager.</returns>
        public static Inventory main
        {
            get
            {
                ThrowIfNotInitialized();
                return m_Inventories[k_MainInventoryHash];
            }
        }

        /// <summary>
        /// This is the 'wallet' Inventory Id as a Hash for quick lookup and compares.
        /// </summary>
        /// <returns>The 'wallet' Inventory id as a hash for quick lookup and compares.</returns>
        internal static int walletInventoryHash { get; } = k_WalletInventoryHash;

        /// <summary>
        /// This is a reference to the "wallet" Inventory for the InventoryManager.
        /// </summary>
        /// <returns>The "wallet" Inventory for the InventoryManager.</returns>
        public static Inventory wallet
        {
            get
            {
                ThrowIfNotInitialized();
                return m_Inventories[k_WalletInventoryHash];
            }
        }

        /// <summary>
        /// Returns an array of all inventories in the manager.
        /// </summary>
        /// <returns>An array of all inventories in the manager.</returns>
        public static Inventory[] GetInventories()
        {
            ThrowIfNotInitialized();

            if (m_Inventories == null)
                return null;

            Inventory[] inventories = new Inventory[m_Inventories.Count];
            m_Inventories.Values.CopyTo(inventories, 0);
            return inventories;
        }

        /// <summary>
        /// Fills the given list with all inventories in the manager.
        /// </summary>
        /// <param name="inventories">The list to fill up.</param>
        public static void GetInventories(List<Inventory> inventories)
        {
            ThrowIfNotInitialized();

            if (m_Inventories == null || inventories == null)
                return;

            inventories.AddRange(m_Inventories.Values);
        }

        // <Inventory Definition Hash, Inventory Instance>
        private static Dictionary<int, Inventory> m_Inventories = null;

        /// <summary>
        /// This is a delegate that takes in a single Inventory as the parameter
        /// </summary>

        public delegate void InventoryEvent(Inventory inventory);

        /// <summary>
        /// This is a Unity event that effects the entire InventoryManager
        /// </summary>
        public class InventoryManagerEvent : UnityEvent {}

        static InventoryManager()
        {
        }

        internal static bool Initialize(ISerializableData data = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("InventoryManager is already initialized and cannot be initialized again.");
                return false;
            }
            
            if (catalog.GetCollectionDefinition(InventoryCatalog.k_MainInventoryDefinitionId) == null ||
                catalog.GetCollectionDefinition(InventoryCatalog.k_WalletInventoryDefinitionId) == null)
            {
                throw new ArgumentException("InventoryManager cannot be Initialized since built-in inventories doesn't exist on InventoryCatalog.");
            }
            
            m_Inventories = new Dictionary<int, Inventory>();
            
            m_IsInitialized = true;

            if (data == null)
            {
                CreateBuiltInInventories();
                AddDefaultInventories();
            }
            else
            {
                m_IsInitialized = FillFromInventoriesData(data);
            }

            return m_IsInitialized;
        }
        
        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            RemoveAllInventories(true, true);
            
            m_Inventories = null;
            m_IsInitialized = false;
        }

        internal static bool FillFromInventoriesData(ISerializableData data)
        {
            if (data == null)
                return false;
            
            var inventoryManagerData = ((GameFoundationSerializableData) data).inventoryManagerData;
            if (inventoryManagerData == null)
                return false;
            
            var mainData = inventoryManagerData.GetInventory(InventoryCatalog.k_MainInventoryDefinitionId);
            if (mainData == null || string.IsNullOrEmpty(mainData.definitionId) ||
                string.IsNullOrEmpty(mainData.inventoryId) || mainData.gameItemLookupId == 0)
            {
                Debug.LogWarning("Persistence Data data doesn't contain Main Inventory.");
                return false;
            }
            
            var walletData = inventoryManagerData.GetInventory(InventoryCatalog.k_WalletInventoryDefinitionId);
            if (walletData == null || string.IsNullOrEmpty(walletData.definitionId) ||
                string.IsNullOrEmpty(walletData.inventoryId) || walletData.gameItemLookupId == 0)
            {
                Debug.LogWarning("Persistence Data data doesn't contain Wallet Inventory.");
                return false;
            }
            
            // remove non built-in inventories
            RemoveAllInventories(true, true);
            
            foreach (var persistedInventory in inventoryManagerData.inventories)
            {
                if (string.IsNullOrEmpty(persistedInventory.definitionId) || string.IsNullOrEmpty(persistedInventory.inventoryId) || persistedInventory.gameItemLookupId == 0)
                {
                    continue;
                }
                
                var inventoryDefinition = catalog.GetCollectionDefinition(persistedInventory.definitionId);
                if (inventoryDefinition == null)
                    continue;
                
                Inventory currentInventory = CreateInventory(inventoryDefinition, persistedInventory.inventoryId, persistedInventory.gameItemLookupId);
                if (currentInventory == null)
                    continue;

                foreach (var item in persistedInventory.items)
                {
                    if (string.IsNullOrEmpty(item.definitionId) || item.gameItemLookupId == 0)
                        continue;

                    if (!currentInventory.ContainsItem(item.definitionId))
                    {
                        int itemHash = Tools.StringToHash(item.definitionId);
                        currentInventory.AddItem(itemHash, item.quantity, item.gameItemLookupId);
                    }
                    else

                    {
                        currentInventory.SetQuantity(item.definitionId, item.quantity);
                    }
                }
            }
            
            return true;
        }

        internal static InventoryManagerSerializableData GetSerializableData()
        {
            var inventoriesIndex = 0;

            // Temp hands conversion while waiting for a better serialization
            var inventoriesArray = GetInventories();
            var inventories = new InventorySerializableData[inventoriesArray.Length];
            foreach (var inventory in inventoriesArray)
            {
                var inventoryItems = inventory.GetItems();
                var itemsData = new InventoryItemSerializableData[inventoryItems.Length];
                var itemsIndex = 0;
                foreach (var item in inventoryItems)
                {
                    itemsData[itemsIndex] = new InventoryItemSerializableData(item.definitionId, item.quantity, item.gameItemId);
                    itemsIndex++;
                }

                inventories[inventoriesIndex] = new InventorySerializableData(inventory.collectionDefinitionId, inventory.id, itemsData, inventory.gameItemId);
                inventoriesIndex++;
            }

            var inventoryManagerData = new InventoryManagerSerializableData(inventories);
            return inventoryManagerData;
        }

        /// <summary>
        /// Returns the current initialization state of the InventoryManager.
        /// </summary>
        /// <returns>The current initialization state of the InventoryManager.</returns>
        public static bool IsInitialized
        {
            get { return m_IsInitialized; }
        }
        private static bool m_IsInitialized = false;

        /// <summary>
        /// Throws an exception if the InventoryManager has not been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not initialized.</exception>
        public static void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() MUST be called before the InventoryManager is used.");
            }
        }

        /// <summary>
        /// This is the InventoryCatalog the InventoryManager uses.
        /// </summary>
        /// <returns>The InventoryCatalog the InventoryManager uses.</returns>
        public static InventoryCatalog catalog
        {
            get
            {
                return GameFoundationSettings.database.inventoryCatalog;
            }
        }

        /// <summary>
        /// Fired whenever a new Inventory is added.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever a new Inventory is added.</returns>
        public static InventoryEvent onInventoryAdded
        {
            get { return m_OnInventoryAdded; }
            set { m_OnInventoryAdded = value; }
        }
        private static InventoryEvent m_OnInventoryAdded;

        /// <summary>
        /// Fired whenever the InventoryManager is reset.
        /// </summary>
        /// <returns>The InventoryManagerEvent fired whenever the InventoryManager is reset.</returns>
        public static InventoryManagerEvent onInventoryManagerReset
        {
            get { return m_OnInventoryManagerReset; }
        }
        private static InventoryManagerEvent m_OnInventoryManagerReset = new InventoryManagerEvent();

        /// <summary>
        /// Fired whenever an Inventory is unable to add items because it's full (i.e. max quantity exceeded).
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is unable to add items because it's full.</returns>
        public static InventoryEvent onInventoryOverflow
        {
            get { return m_OnInventoryOverflow; }
            set { m_OnInventoryOverflow = value; }
        }
        private static InventoryEvent m_OnInventoryOverflow;

        /// <summary>
        /// Fired whenever an Inventory is unable to deduct items because it's empty (i.e. attempts to go BELOW 0 qty).
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is unable to deduct items because it's empty.</returns>
        public static InventoryEvent onInventoryUnderflow
        {
            get { return m_OnInventoryUnderflow; }
            set { m_OnInventoryUnderflow = value; }
        }
        private static InventoryEvent m_OnInventoryUnderflow;

        /// <summary>
        /// Fired whenever an Inventory is about to be removed.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is about to be removed.</returns>
        public static InventoryEvent onInventoryWillRemove
        {
            get { return m_OnInventoryWillRemove; }
            set { m_OnInventoryWillRemove = value; }
        }
        private static InventoryEvent m_OnInventoryWillRemove;

        /// <summary>
        /// Fired whenever an Inventory is removed.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is removed.</returns>
        public static InventoryEvent onInventoryRemoved
        {
            get { return m_OnInventoryRemoved; }
            set { m_OnInventoryRemoved = value; }
        }
        private static InventoryEvent m_OnInventoryRemoved;

        /// <summary>
        /// Returns a unique Inventory Id that hasn't been registered to any existing Inventory. Use with CreateInventory().
        /// </summary>
        /// <returns>Unique Inventory Id</returns>
        public static string GetNewInventoryId()
        {
            ThrowIfNotInitialized();

            string inventoryId;
            // find a unique Collection Id to use (should only need 1 try, but there's a 1 in 4 billion chance that we're unlucky, so...
            do
            {
                inventoryId = Guid.NewGuid().ToString();
            }
            while (m_Inventories.ContainsKey(Tools.StringToHash(inventoryId)));

            return inventoryId;
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition Id to use.
        /// </summary>
        /// <param name="inventoryDefinitionId">The Id of the InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The Id this inventory will have.</param>
        /// <returns>The newly created Inventory based on specified InventoryDefinition.</returns>
        public static Inventory CreateInventory(string inventoryDefinitionId, string inventoryId)
        {
            ThrowIfNotInitialized();

            return CreateInventory(Tools.StringToHash(inventoryDefinitionId), inventoryId);
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition to use by Hash.
        /// </summary>
        /// <param name="inventoryDefinitionHash">The Hash of the InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The Id this Inventory will have.</param>
        /// <returns>The newly created Inventory based on the specified InventoryDefinition.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an invalid Hash is provided.</exception>
        /// <exception cref="ArgumentException">Thrown if the given Id is null, empty, or a duplicate.</exception>
        public static Inventory CreateInventory(int inventoryDefinitionHash, string inventoryId)
        {
            ThrowIfNotInitialized();
            return CreateInventory(inventoryDefinitionHash, inventoryId, 0);
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition to use.
        /// </summary>
        /// <param name="inventoryDefinition">The InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The Id this Inventory will have.</param>
        /// <returns>The newly created Inventory based on the specified InventoryDefinition.</returns>
        public static Inventory CreateInventory(InventoryDefinition inventoryDefinition, string inventoryId)
        {
            ThrowIfNotInitialized();

            if (inventoryDefinition == null)
            {
                Debug.LogWarning("The provided inventory definition is null, this will not be created.");
                return null;
            }

            return CreateInventory(inventoryDefinition, inventoryId, 0);
        }
        
        internal static Inventory CreateInventory(int inventoryDefinitionHash, string inventoryId, int gameItemId)
        {
            return CreateInventory(catalog.GetCollectionDefinition(inventoryDefinitionHash), inventoryId, gameItemId);
        }
        
        internal static Inventory CreateInventory(InventoryDefinition inventoryDefinition, string inventoryId, int gameItemId)
        {
            ThrowIfNotInitialized();
            
            if (inventoryDefinition == null)
            {
                throw new InvalidOperationException("Provided definition is not in the InventoryCatalog.");
            }

            if (string.IsNullOrEmpty(inventoryId))
            {
                throw new ArgumentException("Inventory Id is null or empty, specify an Inventory Id or generate one via GetNewInventoryId()");
            }

            if (m_Inventories.ContainsKey(Tools.StringToHash(inventoryId)))
            {
                throw new ArgumentException("Provided Inventory ID is one that's already registered in this InventoryManager.");
            }

            var newCollection = inventoryDefinition.CreateCollection(inventoryId, inventoryDefinition.displayName, gameItemId);
            m_Inventories.Add(newCollection.hash, newCollection);

            m_OnInventoryAdded?.Invoke(newCollection);

            return newCollection;
        }

        /// <summary>
        /// This will return the Inventory using the specified Inventory Id.
        /// </summary>
        /// <param name="inventoryId">The Id of the Inventory we want.</param>
        /// <returns>The Inventory with the requested Id.</returns>
        public static Inventory GetInventory(string inventoryId)
        {
            ThrowIfNotInitialized();

            return GetInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This will return the Inventory using the specified Inventory Hash.
        /// </summary>
        /// <param name="inventoryHash">The Hash of the Inventory of the Inventory we want.</param>
        /// <returns>The Inventory with the requested  Hash  or null if not found.</returns>
        public static Inventory GetInventory(int inventoryHash)
        {
            ThrowIfNotInitialized();

            Inventory collection;
            m_Inventories.TryGetValue(inventoryHash, out collection);
            return collection;
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given Inventory Id.
        /// </summary>
        /// <param name="inventoryId">The Id we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventory(string inventoryId)
        {
            ThrowIfNotInitialized();

            return HasInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given Inventory Hash.
        /// </summary>
        /// <param name="inventoryHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventory(int inventoryHash)
        {
            ThrowIfNotInitialized();

            return m_Inventories.ContainsKey(inventoryHash);
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given InventoryDefinition Id.
        /// </summary>
        /// <param name="inventoryDefinitionId">The Id we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(string inventoryDefinitionId)
        {
            ThrowIfNotInitialized();

            return HasInventoryByDefinition(Tools.StringToHash(inventoryDefinitionId));
        }

        /// <summary>
        /// This method checks if an Inventory exists for given InventoryDefinition Hash.
        /// </summary>
        /// <param name="inventoryDefinitionHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(int inventoryDefinitionHash)
        {
            ThrowIfNotInitialized();

            foreach (var collection in m_Inventories.Values)
            {
                if (collection.collectionDefinition.hash.Equals(inventoryDefinitionHash))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method checks if an Inventory exists for specified InventoryDefinition.
        /// </summary>
        /// <param name="inventoryDefinition">The InventoryDefinition we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(InventoryDefinition inventoryDefinition)
        {
            ThrowIfNotInitialized();

            if (inventoryDefinition == null)
            {
                return false;
            }

            return HasInventoryByDefinition(inventoryDefinition.hash);
        }

        /// <summary>
        /// This method will remove the Inventory with the given instance Id.
        /// </summary>
        /// <param name="inventoryId">The Id of the Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(string inventoryId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(inventoryId))
                return false;

            return RemoveInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This method will remove the Inventory with the given Inventory Hash.
        /// </summary>
        /// <param name="inventoryHash">The Hash of the Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(int inventoryHash)
        {
            return RemoveInventory(inventoryHash, false);
        }
        
        private static bool RemoveInventory(int inventoryHash, bool forceToRemove)
        {
            ThrowIfNotInitialized();

            if (!forceToRemove && (inventoryHash == k_MainInventoryHash || inventoryHash == k_WalletInventoryHash))
            {
                Debug.LogWarning("Main or Wallet inventories cannot be removed from InventoryManager.");
                return false;
            }

            Inventory inventory;
            if (!m_Inventories.TryGetValue(inventoryHash, out inventory))
            {
                return false;
            }

            if (m_OnInventoryWillRemove != null)
            {
                m_OnInventoryWillRemove.Invoke(inventory);
            }

            if (!m_Inventories.Remove(inventoryHash))
            {
                return false;
            }

            if (m_OnInventoryRemoved != null)
            {
                m_OnInventoryRemoved.Invoke(inventory);
            }

            NotificationSystem.FireNotification(NotificationType.Destroyed, inventory);

            return true;
        }

        /// <summary>
        /// This method will remove the given Inventory.
        /// </summary>
        /// <param name="inventory">The Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(Inventory inventory)
        {
            ThrowIfNotInitialized();

            if (inventory == null)
            {
                return false;
            }

            return RemoveInventory(inventory.hash);
        }

        /// <summary>
        /// This method will remove the Inventory that uses the InventoryDefinition with the given Id.
        /// </summary>
        /// <param name="inventoryDefinitionId">The Id of the InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(string inventoryDefinitionId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(inventoryDefinitionId))
                return 0;

            return RemoveInventoriesByDefinition(Tools.StringToHash(inventoryDefinitionId));
        }

        /// <summary>
        /// This method will remove the Inventory that uses the InventoryDefinition with the given Hash.
        /// </summary>
        /// <param name="inventoryDefinitionHash">The Hash of the InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(int inventoryDefinitionHash)
        {
            ThrowIfNotInitialized();

            List<Inventory> collectionsToRemove = new List<Inventory>();

            foreach (var collection in m_Inventories.Values)
            {
                if (collection.collectionDefinition.hash == inventoryDefinitionHash)
                {
                    collectionsToRemove.Add(collection);
                }
            }

            foreach (var collection in collectionsToRemove)
            {
                if (m_OnInventoryWillRemove != null)
                {
                    m_OnInventoryWillRemove.Invoke(collection);
                }
                
                if (m_Inventories.Remove(collection.hash) && m_OnInventoryRemoved != null)
                {
                    m_OnInventoryRemoved.Invoke(collection);
                }
            }

            return collectionsToRemove.Count;
        }

        /// <summary>
        /// This method will remove the Inventory that uses the given InventoryDefinition.
        /// </summary>
        /// <param name="inventoryDefinition">The InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(InventoryDefinition inventoryDefinition)
        {
            ThrowIfNotInitialized();

            if (inventoryDefinition == null)
            {
                return 0;
            }

            return RemoveInventoriesByDefinition(inventoryDefinition.hash);
        }

        /// <summary>
        /// This will simply clear out all Inventories.
        /// </summary>
        /// <param name="removeDefaultInventories">Whether or not default inventories should also be removed.</param>
        /// <returns>The total number of inventories that were removed.</returns>
        public static int RemoveAllInventories(bool removeDefaultInventories = true)
        {
            ThrowIfNotInitialized();
            
            return RemoveAllInventories(removeDefaultInventories, false);
        }
        
        private static int RemoveAllInventories(bool removeDefaultInventories, bool removeBuiltInInventories)
        {
            // gather a list of all Inventories to remove (needed since we can't iterate a dictionary and remove items as we go)
            var inventoriesToRemove = new List<Inventory>();
            foreach (var inventory in m_Inventories.Values)
            {
                bool remove = false;
                if (removeBuiltInInventories || inventory.hash != k_MainInventoryHash && inventory.hash != k_WalletInventoryHash)
                {
                    remove = true;
                    if (!removeDefaultInventories)
                    {
                        foreach (var defaultCollectionDefinition in catalog.m_DefaultCollectionDefinitions)
                        {
                            if (inventory.hash == defaultCollectionDefinition.hash)
                            {
                                remove = false;
                                break;
                            }
                        }
                    }
                }
                
                if (remove)
                {
                    inventoriesToRemove.Add(inventory);
                }
            }

            foreach (var inventory in inventoriesToRemove)
            {
                RemoveInventory(inventory.hash, removeBuiltInInventories);
            }

            return inventoriesToRemove.Count;
        }

        /// <summary>
        /// Can be called after Initialize() as many times as needed.
        /// Will reset everything to be as it was after Initialize() was called.
        /// </summary>
        public static void Reset()
        {
            ThrowIfNotInitialized();
            
            bool notificationDisabled = NotificationSystem.temporaryDisable;
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = true;
            }

            // Reset default Inventories and remove non default instances
            var inventoriesToRemove = new List<Inventory>();
            foreach (Inventory inventory in m_Inventories.Values)
            {
                bool willRemoved = true;
                if (inventory.hash == k_MainInventoryHash || inventory.hash == k_WalletInventoryHash)
                {
                    willRemoved = false;
                }
                else
                {
                    foreach (var defaultCollectionDefinition in catalog.m_DefaultCollectionDefinitions)
                    {
                        if (inventory.hash == defaultCollectionDefinition.hash)
                        {
                            willRemoved = false;
                            break;
                        }
                    }
                }
                if (willRemoved)
                {
                    inventoriesToRemove.Add(inventory);
                }
                else
                {
                    inventory.Reset();
                }
            }

            foreach (var inventory in inventoriesToRemove)
            {
                RemoveInventory(inventory.hash);
            }

            AddDefaultInventories();

            // invoke event for entire InventoryManager reset
            onInventoryManagerReset?.Invoke();
            
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = false;
            }
        }

        private static void CreateBuiltInInventories()
        {
            if (!m_Inventories.ContainsKey(k_MainInventoryHash))
            {
                CreateInventory(k_MainInventoryHash, InventoryCatalog.k_MainInventoryDefinitionId);
            }
            
            if (!m_Inventories.ContainsKey(k_WalletInventoryHash))
            {
                CreateInventory(k_WalletInventoryHash, InventoryCatalog.k_WalletInventoryDefinitionId);
            }
        }

        // add all default Inventories from the InventoryCatalog
        private static void AddDefaultInventories()
        {
            ThrowIfNotInitialized();

            foreach (var defaultInventoryDefinition in catalog.m_DefaultCollectionDefinitions)
            {
                if (!m_Inventories.ContainsKey(defaultInventoryDefinition.hash))
                {
                    var collectionDefinition = catalog.GetCollectionDefinition(defaultInventoryDefinition.collectionDefinitionHash);
                    if (collectionDefinition != null)
                    {
                        var defaultInventory = collectionDefinition.CreateCollection(defaultInventoryDefinition.id, defaultInventoryDefinition.displayName);
                        m_Inventories[defaultInventory.hash] = defaultInventory;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the given string Hash es to unique value and returns Hash as out variable, if it is.
        /// </summary>
        /// <param name="inventoryId">The Id to checking for.</param>
        /// <returns>True/False whether or not the Hash is unique.</returns>
        public static bool IsInventoryHashUnique(string inventoryId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(inventoryId))
                return false;

            return !m_Inventories.ContainsKey(Tools.StringToHash(inventoryId));
        }
    }
}
