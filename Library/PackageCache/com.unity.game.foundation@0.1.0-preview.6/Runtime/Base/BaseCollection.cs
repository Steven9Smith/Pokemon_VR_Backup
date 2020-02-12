using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A BaseCollection contains data about a certain types of Items at runtime. 
    /// For example, an Inventory is a BaseCollection of InventoryItems, and
    /// the Inventory can be saved and loaded as part of a savegame system.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this Collection uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this Collection uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this Collection uses.</typeparam>
    /// <typeparam name="T4">The type of Items this Collection uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseCollection<T1, T2, T3, T4> : GameItem
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        protected BaseCollection(T1 definition, string id = "") : this(definition, id, 0)
        {
        }

        internal BaseCollection(T1 definition, string id, int gameItemId) : base(definition, id, gameItemId)
        {
            // save off definition used for this Collection
            m_Definition = definition;
            
            // if the Collection is new, it wil create its Default Items
            if (gameItemId == 0)
            {
                // iterate all default Items in the Collection's CollectionDefinition (if any) and add them to the Collection
                AddAllDefaultItems();   
            }
        }

        /// <summary>
        /// This is a delegate that takes in a single Collection as the parameter.
        /// </summary>
        public delegate void BaseCollectionEvent(T2 collection);

        /// <summary>
        /// This is a delegate that takes in a single Item as the parameter.
        /// </summary>
        public delegate void BaseCollectionItemEvent(T4 item);

        private BaseCollectionEvent m_OnCollectionReset;
        private BaseCollectionItemEvent m_OnItemAdded;
        private BaseCollectionItemEvent m_OnItemRemoved;
        private BaseCollectionItemEvent m_OnItemQuantityChanged;
        private BaseCollectionItemEvent m_OnItemQuantityOverflow;

        /// <summary>
        /// Fired whenever a Collection is reset.
        /// </summary>
        /// <returns>BaseCollectionEvent for Collection reset.</returns>
        public BaseCollectionEvent onCollectionReset
        {
            get { return m_OnCollectionReset; }
            set { m_OnCollectionReset = value; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an Item is added to this Collection.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item added.</returns>
        public BaseCollectionItemEvent onItemAdded
        {
            get { return m_OnItemAdded; }
            set { m_OnItemAdded = value; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an Item is removed from this Collection.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item added.</returns>
        public BaseCollectionItemEvent onItemRemoved
        {
            get { return m_OnItemRemoved; }
            set { m_OnItemRemoved = value; }
        }

        /// <summary>
        /// Callback for when an Item intValue has changed.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item quantity changed.</returns>
        public BaseCollectionItemEvent onItemQuantityChanged
        {
            get { return m_OnItemQuantityChanged; }
            set { m_OnItemQuantityChanged = value; }
        }

        /// <summary>
        /// Callback for when an Item intValue has gone above its maximum.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item overflow (quantity too large).</returns>
        public BaseCollectionItemEvent onItemQuantityOverflow
        {
            get { return m_OnItemQuantityOverflow; }
            set { m_OnItemQuantityOverflow = value; }
        }

        [SerializeField]
        private T1 m_Definition;

        /// <summary>
        /// The CollectionDefinition of this Collection which determines the default Items and quantities.
        /// </summary>
        /// <returns>CollectionDefinition for this Collection.</returns>
        public T1 collectionDefinition
        {
            get { return m_Definition; }
        }

        /// <summary>
        /// Helper property for easily accessing the Id of this Collection's CollectionDefinition's Id.
        /// </summary>
        /// <returns>CollectionDefinition Id string for this Collection.</returns>
        public string collectionDefinitionId
        {
            get { return m_Definition?.id; }
        }

        /// <summary>
        /// Helper property for easily accessing the Hash of this Collection's CollectionDefinition's Hash.
        /// </summary>
        /// <returns>CollectionDefinition Hash for this Collection.</returns>
        public int collectionDefinitionHash
        {
            get { return m_Definition != null ? m_Definition.hash : 0; }
        }

        private Dictionary<int, T4> m_ItemsInCollection = new Dictionary<int, T4>();

        /// <summary>
        /// Dictionary of all Items in this collection. Maps ItemDefinition Hash to BaseItem
        /// </summary>
        protected Dictionary<int, T4> itemsInCollection
        {
            get => m_ItemsInCollection;
            set => m_ItemsInCollection = value;
        }

        /// <summary>
        /// Returns an array of all game items in this collection.
        /// </summary>
        /// <returns>The array of all game items in this collection.</returns>
        public T4[] GetItems()
        {
            if (m_ItemsInCollection == null)
                return null;

            T4[] allitems = new T4[m_ItemsInCollection.Count];
            m_ItemsInCollection.Values.CopyTo(allitems, 0);
            return allitems;
        }

        /// <summary>
        /// Fills in the given array of items with the items in this collection.
        /// </summary>
        /// <param name="items">The given list that will be filled with this collection's items.</param>
        public void GetItems(List<T4> items)
        {
            if (m_ItemsInCollection == null || items == null)
                return;
            
            items.AddRange(m_ItemsInCollection.Values);
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition Id string.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are searching for.</param>
        /// <returns>Specified Item by ItemDefinition Id string.</returns>
        public T4 this[string itemDefinitionId]
        {
            get { return GetItem(itemDefinitionId); }
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition we are searching for.</param>
        /// <returns>Specified Item by ItemDefinition Hash.</returns>
        public T4 this[int itemDefinitionHash]
        {
            get { return GetItem(itemDefinitionHash); }
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition
        /// </summary>
        /// <param name="itemDefinition">The Item we are searching for</param>
        /// <returns>Specified Item by ItemDefinition.</returns>
        public T4 this[T3 itemDefinition]
        {
            get { return GetItem(itemDefinition); }
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition as an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added, or null if ItemDefinition is invalid.</returns>
        public T4 AddItem(T3 itemDefinition, int quantity = 1)
        {
            if (itemDefinition == null)
            {
                Debug.LogWarning("Null definition given, this will not be added to the collection.");
                return null;
            }
            
            return AddItem(itemDefinition.hash, quantity);
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition by Id as an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added, or null if definitionId is invalid.</returns>
        public T4 AddItem(string itemDefinitionId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
            {
                Debug.LogWarning("Null or empty id given, this will not be added to the collection.");
                return null;
            }
            
            return AddItem(Tools.StringToHash(itemDefinitionId), quantity);
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition by Hash an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added.</returns>
        public virtual T4 AddItem(int itemDefinitionHash, int quantity = 1)
        {
            return AddItem(itemDefinitionHash, quantity, 0);
        }

        internal virtual T4 AddItem(int itemDefinitionHash, int quantity, int gameItemId)
        {
            T4 itemToReturn;
            
            if (ContainsItem(itemDefinitionHash))
            {
                itemToReturn = m_ItemsInCollection[itemDefinitionHash];
                SetIntValue(itemDefinitionHash, itemToReturn.intValue + quantity);
            }
            else
            {
                BaseItemDefinition<T1, T2, T3, T4> itemDefinition = GetItemDefinition(itemDefinitionHash);
                if (itemDefinition == null)
                {
                    return null;
                }
                
                itemToReturn = itemDefinition.CreateItem(this, gameItemId);
                itemToReturn.intValue = quantity;
                m_ItemsInCollection.Add(itemDefinitionHash, itemToReturn);
            }

            if (onItemQuantityChanged != null)
            {
                onItemQuantityChanged.Invoke(itemToReturn);
            }

            if (onItemAdded != null)
            {
                onItemAdded.Invoke(itemToReturn);
            }
            // TODO: Check if intValue is overflowing and call OnItemQuantityOverflow Event, also contrain quantity
            
            return itemToReturn;
        }

        /// <summary>
        /// Gets an ItemDefinition by its Hash from this Collection.
        /// </summary>
        /// <param name="itemDefinitionHash">Hash of Item Definition to get</param>
        /// <returns>Reference to Item Definition requested</returns>
        protected abstract BaseItemDefinition<T1, T2, T3, T4> GetItemDefinition(int itemDefinitionHash);

        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition reference if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="itemDefinition">A reference to the ItemDefinition of the Item we are getting.</param>
        /// <returns>The reference to the Item instance or null if not found.</returns>
        public T4 GetItem(T3 itemDefinition)
        {
            if (itemDefinition == null)
            {
                return null;
            }

            return GetItem(itemDefinition.hash);
        }
        
        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are getting.</param>
        /// <returns>The reference to the Item instance or null if not found.</returns>
        public T4 GetItem(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
                return null;
            
            return GetItem(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition Hash if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition to return.</param>
        /// <returns>The reference to the Item instance or null if not found.</returns>
        public T4 GetItem(int itemDefinitionHash)
        {
            T4 item;
            m_ItemsInCollection.TryGetValue(itemDefinitionHash, out item);
            return item;
        }

        /// <summary>
        /// This will return all Items that have the given Category (by CategoryDefinition id string)
        /// </summary>
        /// <param name="categoryId">The id of the Category we are checking for.</param>
        /// <returns>An array of the Items that have the given Category.</returns>
        public T4[] GetItemsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return null;
            
            return GetItemsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will put all items that have the given category into the given list of items.
        /// </summary>
        /// <param name="categoryId">The hash of the Category we are checking for.</param>
        /// <param name="items">The list of items to put the results into.</param>
        public void GetItemsByCategory(string categoryId, List<T4> items)
        {
            if (string.IsNullOrEmpty(categoryId))
                return;
            
            GetItemsByCategory(Tools.StringToHash(categoryId), items);
        }

        /// <summary>
        /// This will return all Items that have the given Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we are checking for.</param>
        /// <returns>An array of the Items that have the given Category.</returns>
        public T4[] GetItemsByCategory(int categoryHash)
        {
            List<T4> toReturn = new List<T4>();
            foreach (var keyItemPair in m_ItemsInCollection)
            {
                var definition = keyItemPair.Value.definition;

                if (definition == null)
                    continue;

                foreach (var category in definition.GetCategories())
                {
                    if (category.hash == categoryHash)
                    {
                        toReturn.Add(keyItemPair.Value);
                    }
                }
            }

            return toReturn.ToArray();
        }

        /// <summary>
        /// This will put all items that have the given category into the given list of items.
        /// </summary>
        /// <param name="categoryHash">The hash of the Category we are checking for.</param>
        /// <param name="items">The list of items to put the results into.</param>
        public void GetItemsByCategory(int categoryHash, List<T4> items)
        {
            if (items == null)
            {
                return;
            }
            
            var retrievedItems = GetItemsByCategory(categoryHash);

            if (retrievedItems == null)
            {
                return;
            }

            items.AddRange(retrievedItems);
        }

        /// <summary>
        /// This will return all Items that have the given Category through.
        /// </summary>
        /// <param name="category">The CategoryDefinition we are checking for.</param>
        /// <returns>An array of the Items that have the given Category.</returns>
        public T4[] GetItemsByCategory(CategoryDefinition category)
        {
            if (category == null)
                return null;
            
            return GetItemsByCategory(category.hash);
        }

        /// <summary>
        /// This will put all items that have the given category into the given list of items.
        /// </summary>
        /// <param name="category">The category we are checking for.</param>
        /// <param name="items">The list of items to put the results into.</param>
        public void GetItemsByCategory(CategoryDefinition category, List<T4> items)
        {
            if (category == null)
                return;

            GetItemsByCategory(category.hash, items);
        }

        /// Remove quantity amount of items from item with ItemDefinition Id definitionId.
        /// If the amount would leave the affected item with less than 0 value, it is removed from the collection.
        /// </summary>
        /// <param name="itemDefinitionId">Item Definition Id of the item we are decrementing quantity or removing from the collection.</param>
        /// <param name="quantity">Proposed amount to decrement.</param>
        /// <returns>Whether the item has been removed from the collection</returns>
        public bool RemoveItem(string itemDefinitionId, int quantity)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
                return false;
            
            return RemoveItem(Tools.StringToHash(itemDefinitionId), quantity);
        }

        /// <summary>
        /// Remove quantity amount of items from InventoryItem with ItemDefinition Hash definitionHash.
        /// If the amount would leave the affected item with less than 0 value, it is removed from the collection.
        /// </summary>
        /// <param name="itemDefinitionHash">Item Definition Hash of the item we are decrementing quantity or removing from the collection.</param>
        /// <param name="quantity">Proposed amount to decrement.</param>
        /// <returns>Whether the item has been removed from the collection</returns>
        public bool RemoveItem(int itemDefinitionHash, int quantity)
        {
            if (quantity == 0)
            {
                return false;
            }
            
            T4 item = GetItem(itemDefinitionHash);
            if (item != null)
            {
                if (quantity >= item.intValue)
                {
                    return RemoveItem(itemDefinitionHash);
                }
                SetIntValue(itemDefinitionHash, item.intValue - quantity);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Removes an InventoryItem entry of the specified Item by ItemDefinition Id.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are removing.</param>
        /// <returns>True if item was removed from the collection.</returns>
        public bool RemoveItem(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
                return false;
            
            return RemoveItem(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// Removes an InventoryItem entry of the specified ItemDefinition by Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition we are removing.</param>
        /// <returns>True if item was removed from the collection.</returns>
        public bool RemoveItem(int itemDefinitionHash)
        {
            T4 item = GetItem(itemDefinitionHash);
            if (item != null)
            {
                bool removed = m_ItemsInCollection.Remove(itemDefinitionHash);
                if (removed)
                {
                    NotificationSystem.FireNotification(NotificationType.Destroyed, item);
                    if (onItemRemoved != null)
                    {
                        onItemRemoved.Invoke(item);
                    }
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// This will remove all Items that have the given Category (by  CategoryDefinition Id string).
        /// </summary>
        /// <param name="categoryId">The Id of the CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return 0;
            
            return RemoveItemsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will remove all Items that have the given Category (by  CategoryDefinition Hash ).
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(int categoryHash)
        {
            List<int> toRemove = new List<int>();       //TODO: algorithm allocates list--refactor to avoid new list, if possible. UPDATE: After some research, I believe a list will be necessary for a reasonable solution.
            foreach (var keyItemPair in m_ItemsInCollection)
            {
                var definition = keyItemPair.Value.definition;

                if (definition == null)
                    continue;
                
                foreach (var category in definition.GetCategories())
                {
                    if (category.hash == categoryHash)
                    {
                        toRemove.Add(definition.hash);
                    }
                }
            }

            foreach (var item in toRemove)
            {
                RemoveItem(item);
            }

            return toRemove.Count;
        }

        /// <summary>
        /// This will remove all Items that have the given Category by CategoryDefinition.
        /// </summary>
        /// <param name="category">The CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(CategoryDefinition category)
        {
            if (category == null)
            {
                return 0;
            }
            
            return RemoveItemsByCategory(category.hash);
        }

        /// <summary>
        /// Removes all Items from this Collection.
        /// </summary>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveAll()
        {
            if (m_ItemsInCollection.Count == 0)
            {
                return 0;
            }

            var itemsToRemove = new T4[m_ItemsInCollection.Count];

            // save off all Items in Collection in case event causes dictionary to change
            // this is safer and allows firing did-remove events after dictionary has been cleared
            int itemOn = 0;
            foreach (var item in m_ItemsInCollection.Values)
            {
                itemsToRemove[itemOn++] = item;
            }

            // clear all Items
            m_ItemsInCollection.Clear();

            // fire 'removed' events for all Items just removed
            if (onItemRemoved != null)
            {
                foreach (var item in itemsToRemove)
                {
                    onItemRemoved.Invoke(item);
                }
            }

            return itemsToRemove.Length;
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition (by Id) exists within this Collection.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))	
                return false;
            
            return ContainsItem(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition Hash exists within this Collection.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(int itemDefinitionHash)
        {
            return m_ItemsInCollection.ContainsKey(itemDefinitionHash);
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition exists within this Collection.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(T3 itemDefinition)
        {
            if (itemDefinition == null)
            {
                return false;
            }
            
            return ContainsItem(itemDefinition.hash);
        }

        /// <summary>
        /// Sets the value of the instance within this Collection of the specified ItemDefinition by Id
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the ItemDefinition we are checking for.</param>
        /// <param name="value">The new value we are setting.</param>
        /// <exception cref="ArgumentException">If the given ItemDefinition string is null or empty.</exception>
        protected void SetIntValue(string itemDefinitionId, int value)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
            {
                throw new ArgumentException("The given definition id was null, cannot set value.");
            }
            
            SetIntValue(Tools.StringToHash(itemDefinitionId), value);
        }

        /// <summary>
        /// Sets the value of the instance within this Collection of the specified ItemDefinition by Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the ItemDefinition we are checking for.</param>
        /// <param name="value">The new value we are setting.</param>
        /// <exception cref="ArgumentException">If the given Hash is not a valid entry.</exception>
        protected void SetIntValue(int itemDefinitionHash, int value)
        {
            if (!ContainsItem(itemDefinitionHash))
            {
                throw new ArgumentException("The given definition hash was not found, cannot set value.");
            }

            var item = GetItem(itemDefinitionHash);
            item.intValue = value;

            if (onItemQuantityChanged != null)
            {
                onItemQuantityChanged.Invoke(item);
            }

            // TODO: When/if quantity becomes a stat, this may need to be removed if stats end up having their own notification fire calls.
            NotificationSystem.FireNotification(NotificationType.Modified, item);
            // TODO: Check if intValue is overflowing and call OnItemQuantityOverflow Event
        }

        /// <summary>
        /// Resets the contents of this Collection based on the CollectionDefinition.
        /// </summary>
        public void Reset()
        {
            bool notificationDisabled = NotificationSystem.temporaryDisable;
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = true;
            }
            
            // remove all existing Items from the Collection
            RemoveAll();

            // fire reset event
            if (onCollectionReset != null)
            {
                onCollectionReset.Invoke((T2)this);
            }

            // iterate all default Items in the CollectionDefinition (if there are any) and add them to the Collection
            AddAllDefaultItems();
            
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = false;
            }
        }

        // iterate all default Items in the CollectionDefinition (if there are any) and add them to the Collection
        protected void AddAllDefaultItems()
        {
            bool notificationDisabled = NotificationSystem.temporaryDisable;
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = true;
            }
            
            if (m_Definition != null)
            { 
                var defaultItems = m_Definition.GetDefaultItems();
                if (defaultItems != null)
                {
                    foreach (var defaultItem in defaultItems)
                    {
                        AddItem(defaultItem.definitionHash, defaultItem.quantity);
                    }
                }
            }
            
            if (!notificationDisabled)
            {
                NotificationSystem.temporaryDisable = false;
            }
        }
    }
}
