using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes preset values and rules for a Collection by using a CollectionDefinition.
    /// During runtime, it may be useful to refer back to the CollectionDefinition for the presets and rules, 
    /// but the values cannot be changed at runtime (your system may, for example, bypass the presets, 
    /// or calculate new values on the fly with modifiers).
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this CollectionDefinition uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseCollectionDefinition<T1, T2, T3, T4> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        protected BaseCollectionDefinition()
        {
        }

        [SerializeField]
        private List<DefaultItem> m_DefaultItems = new List<DefaultItem>();

        /// <summary>
        /// Items that are added when the collection is created
        /// </summary>
        protected List<DefaultItem> defaultItems
        {
            get => m_DefaultItems;
            set => m_DefaultItems = value;
        }

        /// <summary>
        /// Returns an array of the default items in this collection definition.
        /// </summary>
        /// <returns>An array of the default items in this collection definition.</returns>
        public DefaultItem[] GetDefaultItems()
        {
            if (m_DefaultItems == null)
                return null;
            
            return m_DefaultItems.ToArray();
        }

        /// <summary>
        /// Fills the given list with all default items in this collection definition.
        /// </summary>
        /// <param name="defaultItems">The list to fill up.</param>
        public void GetDefaultItems(List<DefaultItem> defaultItems)
        {
            if (m_DefaultItems == null || defaultItems == null)
                return;
            
            defaultItems.AddRange(m_DefaultItems);
        }

        /// <summary>
        /// Adds the given DefaultItem to this CollectionDefinition.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <param name="quantity">Quantity of Items to add (defaults to 0 which creates the Item with zero quantity).</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public virtual bool AddDefaultItem(T3 defaultItem, int quantity = 0)
        {
            if (defaultItem == null)
                return false;
            
            return AddDefaultItem(new DefaultItem(defaultItem.hash, quantity));
        }

        /// <summary>
        /// Adds the given DefaultItem to this CollectionDefinition.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public virtual bool AddDefaultItem(DefaultItem defaultItem)
        {
            Tools.ThrowIfPlayMode("Cannot add a defaultItem to a CollectionDefinition while in play mode.");
            
            if (m_DefaultItems.Contains(defaultItem))
            {
                return false;
            }

            m_DefaultItems.Add(defaultItem);

            return true;
        }

        /// <summary>
        /// Sets the default quantity of the Item specified.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem we are changing quantity of.</param>
        /// <param name="quantity">The quantity to change to.</param>
        /// <returns>Bool of whether changing quantity was successful.</returns>
        public bool SetDefaultItemQuantity(DefaultItem defaultItem, int quantity)
        {
            Tools.ThrowIfPlayMode("Cannot set DefaultItem quantity while in play mode.");

            int index = m_DefaultItems.IndexOf(defaultItem);
            if (index < 0 || index >= m_DefaultItems.Count)
            {
                return false;
            }

            defaultItem.quantity = quantity;
            m_DefaultItems[index] = defaultItem;

            return true;
        }

        /// <summary>
        /// Removes the specified DefaultItem from this CollectionDefinition's list of DefaultItems.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveDefaultItem(DefaultItem defaultItem)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DefaultItem from a CollectionDefinition while in play mode.");

            return m_DefaultItems.Remove(defaultItem);
        }

        /// <summary>
        /// Swaps the locations of the DefaultItems in the defaultItems list.
        /// </summary>
        /// <param name="defaultItem1">The first DefaultItem to swap.</param>
        /// <param name="defaultItem2">The second DefaultItem to swap.</param>
        /// <returns> Returns a bool value specifying whether the swap was successful. 
        /// Swap will fail if either item is null, not in the defaultItems list or if both items are the same.</returns>
        public bool SwapDefaultItemsListOrder(DefaultItem defaultItem1, DefaultItem defaultItem2)
        {
            Tools.ThrowIfPlayMode("Cannot swap DefaultItems order while in play mode.");

            if (defaultItem1 == null || defaultItem2 == null)
            {
                return false;
            }

            int index1 = m_DefaultItems.IndexOf(defaultItem1);
            int index2 = m_DefaultItems.IndexOf(defaultItem2);

            if (index1 < 0 || index2 < 0 || index1 == index2)
            {
                return false;
            }

            m_DefaultItems[index1] = defaultItem2;
            m_DefaultItems[index2] = defaultItem1;

            return true;
        }

        /// <summary>
        /// Spawns an instance of a Collection that is based off of this CollectionDefinition.
        /// </summary>
        /// <returns>The reference to the newly created Collection.</returns>
        internal abstract T2 CreateCollection(string collectionId, string displayName, int gameItemId = 0);
    }
}
