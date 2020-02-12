using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is a class for storing Definitions for a system that the user setup in the editor.
    /// Derived classes will specify each generic to specify which classes are used by their Catalog.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this Catalog uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T4">The type of Items this Catalog uses.</typeparam>
    public abstract class BaseCatalog<T1, T2, T3, T4> : ScriptableObject
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        [SerializeField]
        private List<CategoryDefinition> m_Categories = new List<CategoryDefinition>();

        /// <summary>
        /// List of CategoryDefinitions inside this Catalog
        /// </summary>
        protected List<CategoryDefinition> categories
        {
            get => m_Categories;
            set => m_Categories = value;
        }
        
        [SerializeField]
        private List<T1> m_CollectionDefinitions = new List<T1>();

        /// <summary>
        /// A list of all CollectionDefinition this Catalog can use.
        /// </summary>
        protected List<T1> collectionDefinitions
        {
            get => m_CollectionDefinitions;
            set => m_CollectionDefinitions = value;
        }

        [SerializeField]
        private List<T3> m_ItemDefinitions = new List<T3>();

        /// <summary>
        /// A list of each type of ItemDefinition this Catalog can use.
        /// </summary>
        protected List<T3> itemDefinitions
        {
            get => m_ItemDefinitions;
            set => m_ItemDefinitions = value;
        }
        
        /// <summary>
        /// A list of DefaultCollectionDefinitions in this Catalog.
        /// </summary>
        [SerializeField]
        internal List<DefaultCollectionDefinition> m_DefaultCollectionDefinitions = new List<DefaultCollectionDefinition>();

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryId">The Id of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid Hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition or null if the Hash is not found.</returns>
        public CategoryDefinition GetCategory(int categoryHash)
        {
            foreach (CategoryDefinition definition in m_Categories)
            {
                if (definition.hash == categoryHash)
                {
                    return definition;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Returns an array of all categories in this catalog.
        /// </summary>
        /// <returns>An array of all categories.</returns>
        public CategoryDefinition[] GetCategories()
        {
            if (m_Categories == null)
            {
                return null;
            }
            
            return m_Categories.ToArray();
        }

        /// <summary>
        /// Fills in the given list with all categories in this catalog.
        /// </summary>
        /// <param name="categories">The list to fill up with categories.</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (m_Categories == null || categories == null)
            {
                return;
            }
            
            categories.AddRange(m_Categories);
        }

        /// <summary>
        /// Adds the given CategoryDefinition to this Catalog.
        /// </summary>
        /// <param name="category">The CategoryDefinition to add.</param>
        /// <returns>Whether or not the CategoryDefinition was added successfully.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate entry is given.</exception>
        public bool AddCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot add a CategoryDefinition to a Catalog while in play mode.");

            if (category == null)
            {
                return false;
            }
            
            if (GetCategory(category.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + category.id + ", hash: " + category.hash + ")");
            }
            
            m_Categories.Add(category);
            return true;
        }

        /// <summary>
        /// Removes the given CategoryDefinition from this Catalog.
        /// </summary>
        /// <param name="category">The CategoryDefinition to remove.</param>
        /// <returns>Whether or not the CategoryDefinition was successfully removed.</returns>
        public bool RemoveCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot remove a category from a Catalog while in play mode.");
            
            return m_Categories.Remove(category);
        }

        /// <summary>
        /// Returns an array of all collection definitions in the catalog.
        /// </summary>
        /// <returns>An array of all collection definitions in the catalog.</returns>
        public T1[] GetCollectionDefinitions()
        {
            if (m_CollectionDefinitions == null)
            {
                return null;
            }
            
            return m_CollectionDefinitions.ToArray();
        }

        /// <summary>
        /// Adds all collection definitions into the given list.
        /// </summary>
        /// <param name="collectionDefinitions">The list to add collection definitions to.</param>
        public void GetCollectionDefinitions(List<T1> collectionDefinitions)
        {
            if (m_CollectionDefinitions == null || collectionDefinitions == null)
            {
                return;
            }
            
            collectionDefinitions.AddRange(m_CollectionDefinitions);
        }

        /// <summary>
        /// Adds the given CollectionDefinition to this Catalog.
        /// </summary>
        /// <param name="collectionDefinition">The CollectionDefinition to add.</param>
        /// <returns>Whether or not the CollectionDefinition was added successfully.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate entry is given.</exception>
        public bool AddCollectionDefinition(T1 collectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a CollectionDefinition to a Catalog while in play mode.");

            if (collectionDefinition == null)
            {
                return false;
            }

            if (GetCollectionDefinition(collectionDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + collectionDefinition.id + ", hash: " + collectionDefinition.hash + ")");
            }

            m_CollectionDefinitions.Add(collectionDefinition);
            return true;
        }

        /// <summary>
        /// Removes the given CollectionDefinition from this Catalog.
        /// </summary>
        /// <param name="collectionDefinition">The CollectionDefinition to remove.</param>
        /// <returns>Whether or not the CollectionDefinition was successfully removed.</returns>
        public virtual bool RemoveCollectionDefinition(T1 collectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a CollectionDefinition from a Catalog while in play mode.");

            if (collectionDefinition == null || !m_CollectionDefinitions.Contains(collectionDefinition))
            {
                return false;
            }
            
            collectionDefinition.OnRemove();

            return m_CollectionDefinitions.Remove(collectionDefinition);
        }

        /// <summary>
        /// Returns an array of all item definitions.
        /// </summary>
        /// <returns>An array of all item definitions.</returns>
        public T3[] GetItemDefinitions()
        {
            if (m_ItemDefinitions == null)
            {
                return null;
            }
            
            return m_ItemDefinitions.ToArray();
        }

        /// <summary>
        /// Fills in the given list with all item definitions in this catalog.
        /// </summary>
        /// <param name="itemDefinitions">The list to fill up.</param>
        public void GetItemDefinitions(List<T3> itemDefinitions)
        {
            if (m_ItemDefinitions == null || itemDefinitions == null)
            {
                return;
            }
            
            itemDefinitions.AddRange(m_ItemDefinitions);
        }

        /// <summary>
        /// Adds the given ItemDefinition to this Catalog.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate definition is given.</exception>
        public bool AddItemDefinition(T3 itemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add an ItemDefinition to a Catalog while in play mode.");

            if (itemDefinition == null)
            {
                return false;
            }

            if (GetItemDefinition(itemDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + itemDefinition.id + ", hash: " + itemDefinition.hash + ")");
            }
            
            m_ItemDefinitions.Add(itemDefinition);
            return true;
        }

        /// <summary>
        /// Removes the given ItemDefinition from this Catalog.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveItemDefinition(T3 itemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove an ItemDefinition from a Catalog while in play mode.");

            if (itemDefinition == null || !m_ItemDefinitions.Contains(itemDefinition))
            {
                return false;
            }

            itemDefinition.OnRemove();
            
            return m_ItemDefinitions.Remove(itemDefinition);
        }


        /// <summary>
        /// Returns an array of all default collection definitions.
        /// </summary>
        /// <returns>An array of all default collection definitions.</returns>
        public DefaultCollectionDefinition[] GetDefaultCollectionDefinitions()
        {
            if (m_DefaultCollectionDefinitions == null)
            {
                return null;
            }

            return m_DefaultCollectionDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with all default collection definitions in this catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinitions">The list to fill up.</param>
        public void GetDefaultCollectionDefinitions(List<DefaultCollectionDefinition> defaultCollectionDefinitions)
        {
            if (m_DefaultCollectionDefinitions == null || defaultCollectionDefinitions == null)
            {
                return;
            }

            defaultCollectionDefinitions.AddRange(m_DefaultCollectionDefinitions);
        }

        /// <summary>
        /// Adds the given DefaultCollectionDefinition to this Catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinition">The DefaultCollectionDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate default collection definition is given.</exception>
        public bool AddDefaultCollectionDefinition(DefaultCollectionDefinition defaultCollectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a DefaultCollectionDefinition to a Catalog while in play mode.");

            if (defaultCollectionDefinition == null)
            {
                return false;
            }
            
            if (GetDefaultCollectionDefinition(defaultCollectionDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + defaultCollectionDefinition.id + ", hash: " + defaultCollectionDefinition.hash + ")");
            }
            
            m_DefaultCollectionDefinitions.Add(defaultCollectionDefinition);
            return true;
        }

        /// <summary>
        /// Removes the given DefaultCollectionDefinition from this Catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinition">The DefaultCollectionDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveDefaultCollectionDefinition(DefaultCollectionDefinition defaultCollectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DefaultCollectionDefinition from a Catalog while in play mode.");
                
            return m_DefaultCollectionDefinitions.Remove(defaultCollectionDefinition);
        }

        /// <summary>
        /// Find CollectionDefinition by Definition Id.
        /// </summary>
        /// <param name="collectionDefinitionId">The Id of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(string collectionDefinitionId)
        {
            if (string.IsNullOrEmpty(collectionDefinitionId))
            {
                return null;
            }
            
            return GetCollectionDefinition(Tools.StringToHash(collectionDefinitionId));
        }

        /// <summary>
        /// Find CollectionDefinition by Hash.
        /// </summary>
        /// <param name="collectionDefinitionHash">The Hash of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(int collectionDefinitionHash)
        {
            foreach (var collectionDefinition in m_CollectionDefinitions)
            {
                if (collectionDefinition.hash.Equals(collectionDefinitionHash))
                {
                    return collectionDefinition;
                }
            }
            
            return null;
        }

        /// <summary>
        /// This is a getter for getting ItemDefinitions by their Id.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
            {
                return null;
            }
            
            return GetItemDefinition(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// This is a getter for getting ItemDefinitions by their Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(int itemDefinitionHash)
        {
            foreach (var itemDefinition in m_ItemDefinitions)
            {
                if (itemDefinition.hash.Equals(itemDefinitionHash))
                {
                    return itemDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition id.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we want.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            return GetItemDefinitionsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we want.</param>
        /// <param name="items">The list to fill up.</param>
        public void GetItemDefinitionsByCategory(string categoryId, List<T3> items)
        {
            if (string.IsNullOrEmpty(categoryId) || items == null)
            {
                return;
            }
            
            items.AddRange(GetItemDefinitionsByCategory(categoryId));
        }

        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition we want to check for.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(int categoryHash)
        {
            List<T3> items = new List<T3>();
            foreach (var definition in m_ItemDefinitions)
            {
                if (definition != null && definition.GetCategories() != null)
                {
                    foreach (var category in definition.GetCategories())
                    {
                        if (category.hash == categoryHash)
                        {
                            items.Add(definition);
                        }
                    }
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition we want to check for.</param>
        /// <param name="items">The list to fill up.</param>
        public void GetItemDefinitionsByCategory(int categoryHash, List<T3> items)
        {
            if (items == null)
            {
                return;
            }
            
            items.AddRange(GetItemDefinitionsByCategory(categoryHash));
        }

        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition CategoryDefinition.
        /// </summary>
        /// <param name="category">The Category we want to check for.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(CategoryDefinition category)
        {
            if (category == null)
            {
                return null;
            }
            
            return GetItemDefinitionsByCategory(category.hash);
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// </summary>
        /// <param name="category">The Category we want to check for.</param>
        /// <param name="items">The list to fill up.</param>
        public void GetItemDefinitionsByCategory(CategoryDefinition category, List<T3> items)
        {
            if (category == null || items == null)
            {
                return;
            }
            
            items.AddRange(GetItemDefinitionsByCategory(category));
        }

        /// <summary>
        /// This gets the DefaultCollectionDefinitions by Id string.
        /// </summary>
        /// <param name="defaultDefinitionId">The Id of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition GetDefaultCollectionDefinition(string defaultDefinitionId)
        {
            if (string.IsNullOrEmpty(defaultDefinitionId))
            {
                return null;
            }
            
            return GetDefaultCollectionDefinition(Tools.StringToHash(defaultDefinitionId));
        }

        /// <summary>
        /// This gets the DefaultCollectionDefinition by Hash.
        /// </summary>
        /// <param name="defaultDefinitionHash">The Hash of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition GetDefaultCollectionDefinition(int defaultDefinitionHash)
        {
            foreach (var defaultCollectionDefinition in m_DefaultCollectionDefinitions)
            {
                if (defaultCollectionDefinition.hash.Equals(defaultDefinitionHash))
                {
                    return defaultCollectionDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if the given Hash is available to be added to CollectionDefinitions.
        /// </summary>
        /// <param name="collectionDefinitionHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not Hash is available for use.</returns>
        public bool IsCollectionDefinitionHashUnique(int collectionDefinitionHash)
        {
            return GetCollectionDefinition(collectionDefinitionHash) == null;
        }

        /// <summary>
        /// Check if the given Hash is not yet within ItemDefinitions and is available for use.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not Hash is available for use.</returns>
        public bool IsItemDefinitionHashUnique(int itemDefinitionHash)
        {
            return GetItemDefinition(itemDefinitionHash) == null;
        }

        /// <summary>
        /// Check if the given CategoryDefinition is found in this Catalog's list of CategoryDefinitions.
        /// </summary>
        /// <param name="category">The CategoryDefinition we are checking for.</param>
        /// <returns>True if the CategoryDefinition is found, else False.</returns>
        public bool HasCategoryDefinition(CategoryDefinition category)
        {
            if (category == null)
            {
                return false;
            }

            foreach (CategoryDefinition currentCategory in m_Categories)
            {
                if (currentCategory.hash == category.hash)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the given CategoryDefinition display name is found in this Catalog's list of CategoryDefinitions.
        /// </summary>
        /// <param name="categoryName">The display name of the Category we are checking for.</param>
        /// <returns>True if the CategoryDefinition is found.</returns>
        public bool HasCategoryDefinition(string categoryName)
        {
            foreach (CategoryDefinition category in m_Categories)
            {
                if (category.displayName == categoryName)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// This will be called whenever we want to verify that any default values that should exist in a catalog, do exist.
        /// </summary>
        internal abstract void VerifyDefaultCollections();
#endif
    }
}
