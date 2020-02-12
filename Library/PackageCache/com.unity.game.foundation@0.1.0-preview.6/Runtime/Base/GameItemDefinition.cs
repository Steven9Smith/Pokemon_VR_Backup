using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Base class for both BaseItemDefinition and BaseCollectionDefinition. 
    /// Holds Id, dsplay name, etc., and allows DetailDefinitions to be attached as needed.
    /// </summary>
    public class GameItemDefinition : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] 
        private string m_DisplayName;

        /// <summary>
        /// The name of this GameItemDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this GameItemDefinition for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { SetDisplayName(value); }
        }

        private void SetDisplayName(string name)
        {
            Tools.ThrowIfPlayMode("Cannot set the display name of a GameItemDefinition while in play mode.");

            m_DisplayName = name;
        }

        [SerializeField]
        private string m_Id;

        /// <summary>
        /// The string Id of this GameItemDefinition.
        /// </summary>
        /// <returns>The string Id of this GameItemDefinition.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField] 
        private int m_Hash;

        /// <summary>
        /// The Hash of this GameItemDefinition's Id.
        /// </summary>
        /// <returns>The Hash of this GameItemDefinition's Id.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField]
        private GameItemDefinition m_ReferenceDefinition;

        /// <summary>
        /// The reference GameItemDefinition for this GameItemDefinition.
        /// </summary>
        /// <returns>The reference GameItemDefinition for this GameItemDefinition.</returns>
        public GameItemDefinition referenceDefinition
        {
            get { return m_ReferenceDefinition; }
            set { SetReferenceDefinition(value); }
        }

#if UNITY_EDITOR
        private void HandleGameItemCatalogWillRemoveGameItemDefinition(object sender, GameItemDefinition gameItemDefinition)
        {
            if (ReferenceEquals(m_ReferenceDefinition, gameItemDefinition))
            {
                m_ReferenceDefinition = null;
                EditorUtility.SetDirty(this);
            }
        }

        private void OnEnable()
        {
            GameItemCatalog.OnWillRemoveGameItemDefinition += HandleGameItemCatalogWillRemoveGameItemDefinition;
        }

        private void OnDisable()
        {
            GameItemCatalog.OnWillRemoveGameItemDefinition -= HandleGameItemCatalogWillRemoveGameItemDefinition;
        }
#endif

        private void SetReferenceDefinition(GameItemDefinition gameItemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot set the reference GameItemDefinition of a GameItemDefinition while in play mode.");

            if (m_ReferenceDefinition != gameItemDefinition)
            {
                if (ReferenceEquals(this, gameItemDefinition))
                {
                    throw new ArgumentException("GameItemDefinition cannot point to itself.");
                }

                m_ReferenceDefinition = gameItemDefinition;
            }
        }

        [SerializeField]
        private List<int> m_Categories = new List<int>();

        /// <summary>
        /// List of Category hashes assigned to this Game Item Definition 
        /// </summary>
        protected internal List<int> categories
        {
            get { return m_Categories; }
            set { m_Categories = value; }
        }

        /// <summary>
        /// Returns an array of all categories on this game item definition.
        /// </summary>
        /// <returns>An array of all categories on this game item definition.</returns>
        public CategoryDefinition[] GetCategories()
        {
            var categories = QueryCategories();

            if (categories == null)
                return null;

            return categories.ToArray();
        }

        /// <summary>
        /// Fills the given list with all categories on this game item definition.
        /// </summary>
        /// <param name="categories">The list to fill up.</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (categories == null)
                return;

            categories.AddRange(QueryCategories());
        }

        [SerializeField]
        private List<BaseDetailDefinition> m_DetailDefinitionValues = new List<BaseDetailDefinition>();
        
        private Dictionary<Type, BaseDetailDefinition> m_DetailDefinitions = new Dictionary<Type, BaseDetailDefinition>();

        private List<CategoryDefinition> QueryCategories()
        {
            if (m_Categories == null)
                return null;

            List<CategoryDefinition> actualCategories = new List<CategoryDefinition>();
            foreach (int categoryHash in m_Categories)
            {
                CategoryDefinition category = GetCategoryDefinition(categoryHash);

                if (category == null)
                {
                    continue;
                }

                actualCategories.Add(category);
            }
            return actualCategories;
        }

        /// <summary>
        /// Gets a CategoryDefinition from this GameItemDefinition categories with the following Hash 
        /// </summary>
        /// <param name="categoryHash">CategoryDefinition Hash of CategoryDefinition to get</param>
        /// <returns>Requested Category Definition</returns>
        protected virtual CategoryDefinition GetCategoryDefinition(int categoryHash)
        {
            return GameFoundationSettings.database.gameItemCatalog.GetCategory(categoryHash);
        }

        /// <summary>
        /// Adds the given Category to this GameItemDefinition.
        /// </summary>
        /// <param name="category">The CategoryDefinition to add.</param>
        /// <returns>Whether or not adding the Category was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the given category is already on this definition.</exception>
        public bool AddCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot add a CategoryDefinition to a GameItemDefinition while in play mode.");

            if (category == null)
            {
                return false;
            }

            if (m_Categories.Contains(category.hash))
            {
                throw new ArgumentException("Cannot add a duplicate category definition.");
            }
            m_Categories.Add(category.hash);
            return true;
        }

        /// <summary>
        /// Adds the given Categories to this GameItemDefinition by list.
        /// </summary>
        /// <param name="categories">The list of CategoryDefinitions to add.</param>
        public bool AddCategories(List<CategoryDefinition> categories)
        {
            Tools.ThrowIfPlayMode("Cannot add CategoryDefinitions to a GameItemDefinition while in play mode.");

            if (categories == null)
            {
                return false;
            }

            foreach (CategoryDefinition category in categories)
            {
                AddCategory(category);
            }

            return true;
        }

        /// <summary>
        /// Removes the given Category from this GameItemDefinition.
        /// </summary>
        /// <param name="category">The CategoryDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot remove a Category from a GameItemDefinition while in play mode.");

            if (category == null)
            {
                return false;
            }

            return m_Categories.Remove(category.hash);
        }

        /// <summary>
        /// Returns an array of all detail definitions on this game item definition.
        /// </summary>
        /// <returns>An array of all detail definitions on this game item definition.</returns>
        public BaseDetailDefinition[] GetDetailDefinitions()
        {
            if (m_DetailDefinitions == null)
                return null;

            BaseDetailDefinition[] baseDetailDefinitions = new BaseDetailDefinition[m_DetailDefinitions.Count];
            m_DetailDefinitions.Values.CopyTo(baseDetailDefinitions, 0);
            return baseDetailDefinitions;
        }

        /// <summary>
        /// Fills in the given list with all detail definitions on this game item definition.
        /// </summary>
        /// <param name="detailDefinitions">The list to fill up.</param>
        public void GetDetailDefinitions(List<BaseDetailDefinition> detailDefinitions)
        {
            if (m_DetailDefinitions == null || detailDefinitions == null)
            {
                return;
            }

            detailDefinitions.AddRange(m_DetailDefinitions.Values);
        }

        /// <summary>
        /// This will add the given DetailDefinition to this GameItemDefinition.
        /// </summary>
        /// <param name="detailDefinition">The DetailDefinition to add.</param>
        /// <returns>A reference to the DetailDefinition that was just added.</returns>
        /// <exception cref="ArgumentException">Thrown if the given detail definition is already on this game item.</exception>
        public BaseDetailDefinition AddDetailDefinition(BaseDetailDefinition detailDefinition) 
        {
            Tools.ThrowIfPlayMode("Cannot add a DetailDefinition to a GameItemDefinition during play mode.");

            if (detailDefinition == null)
            {
                Debug.LogWarning("Null detail definition given, this will not be added to the definition.");
                return null;
            }

            if (m_DetailDefinitions == null)
            {
                m_DetailDefinitions = new Dictionary<Type, BaseDetailDefinition>();
            }

            // if the Detail already exists then throw
            var detailDefinitionType = detailDefinition.GetType();
            if (m_DetailDefinitions.ContainsKey(detailDefinitionType))
            {
                throw new ArgumentException(string.Format("The definition \"{0}\" already has a {1} detail.", m_Id, detailDefinitionType.Name));
            }
            
            // TODO: Make this into a more general use dependency system for details down the road.
            // Special case, when adding a CurrencyDetail, we automatically want an AnalyticsDetail to be added.
            if (detailDefinition.GetType() == typeof(CurrencyDetailDefinition))
            {
                var hasAnalyticsAlready = GetDetailDefinition<AnalyticsDetailDefinition>();
                if (hasAnalyticsAlready == null)
                {
                    AddDetailDefinition<AnalyticsDetailDefinition>();
                }
            }

            detailDefinition.owner = this;

            m_DetailDefinitions.Add(detailDefinitionType, detailDefinition);

            // naming convention for details objects
            string itemTypeShortName = GetType().Name.Replace("Definition", "");
            string detailsTypeShortName = detailDefinitionType.Name.Replace("DetailDefinition", "");
            detailDefinition.name = $"{m_Id}_{itemTypeShortName}_Detail_{detailsTypeShortName}";

#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(this))
            {
                AssetDatabase.AddObjectToAsset(detailDefinition, this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif

            return detailDefinition;
        }

        /// <summary>
        /// This will add a DetailDefinition of specified type to this GameItemDefinition.
        /// </summary>
        /// <typeparam name="T">The type of the new DetailDefinition to add.</typeparam>
        /// <returns>A reference to the DetailDefinition that was just added.</returns>
        public T AddDetailDefinition<T>() where T : BaseDetailDefinition 
        {
            var newDetailDefinition = CreateInstance<T>();

            AddDetailDefinition(newDetailDefinition);

            return newDetailDefinition;
        }

        /// <summary> 
        /// This will return a reference to the requested DetailDefinition by type.
        /// </summary>
        /// <typeparam name="T">The type of DetailDefinition requested.</typeparam>
        /// <returns>A reference to the DetailDefinition, or null if this GameItemDefinition does not have one.</returns>
        public T GetDetailDefinition<T>(bool lookInReferenceDefinition = true)
            where T : BaseDetailDefinition
        {
            if (m_DetailDefinitions != null && m_DetailDefinitions.ContainsKey(typeof(T)))
            {
                return m_DetailDefinitions[typeof(T)] as T;
            }

            if (lookInReferenceDefinition && referenceDefinition != null)
            {
                return referenceDefinition.GetDetailDefinition<T>();
            }

            return null;
        }

        /// <summary> 
        /// Remove the requested DetailDefinition by type from this GameItemDefinition.
        /// </summary>
        /// <typeparam name="T">The type of DetailDefinition we want to remove.</typeparam>
        public bool RemoveDetailDefinition<T>()
            where T : BaseDetailDefinition
        {
            Tools.ThrowIfPlayMode("Cannot remove a DetailDefinition from a GameItemDefinition during play mode.");

            var detailDefinitionToRemove = GetDetailDefinition<T>(false);

            if (detailDefinitionToRemove == null)
            {
                return false;
            }

            return RemoveDetailDefinition(detailDefinitionToRemove);
        }

        /// <summary>
        /// Removes the specified DetailDefinition from this GameItemDefinition.
        /// </summary>
        /// <param name="detailDefinition">DetailDefinition to remove from this GameItemDefinition.</param>
        /// <returns>Whether or not the given detail was successfully removed.</returns>
        public bool RemoveDetailDefinition(BaseDetailDefinition detailDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DetailDefinition from a GameItemDefinition during play mode.");

            if (detailDefinition == null)
            {
                return false;
            }

            var detailType = detailDefinition.GetType();
            if (!m_DetailDefinitions.ContainsKey(detailType))
            {
                return false;
            }

            m_DetailDefinitions.Remove(detailType);

#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(this))
            {
                AssetDatabase.RemoveObjectFromAsset(detailDefinition);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
            return true;
        }

        /// <summary>
        /// This makes sure that any detail definitions are first
        /// removed from the asset before being removed from the GameItemDefinition.
        /// This ensures that there are no orphaned assets nested in the catalog asset.
        /// </summary>
        /// <returns>Returns an int indicating how many detail definitions were removed.</returns>
        /// <exception cref="Exception">Throws an exception if called during play mode.</exception>
        private int RemoveAllDetailDefinitions()
        {
            if (Application.isPlaying)
            {
                throw new System.Exception("Cannot remove DetailDefinitions from a GameItemDefinition during play mode.");
            }

            int count = m_DetailDefinitions.Count;

            // if any DetailDefinitions are actually attached
            if (count > 0)
            {
#if UNITY_EDITOR
                // remove them from the asset database
                foreach(var detailDefinitionToRemove in m_DetailDefinitions)
                {
                    AssetDatabase.RemoveObjectFromAsset(detailDefinitionToRemove.Value);
                }
#endif

                // clear the list
                m_DetailDefinitions.Clear();

                // save updated asset database
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }

            return count;
        }

        /// <summary>
        /// To be called before destroying a GameItemDefinition.
        /// Use this only to clean up the GameItemDefinition being destroyed.
        /// For example, if it has children that are nested assets,
        /// then make sure to cleanly remove those assets from the catalog asset.
        /// </summary>
        /// <exception cref="Exception">Throws an exception if called during play mode.</exception>
        internal virtual void OnRemove()
        {
            if (Application.isPlaying)
            {
                throw new System.Exception("GameItemDefinitions cannot be removed during play mode.");
            }

            RemoveAllDetailDefinitions();
        }

        /// <summary>
        /// Checks whether or not the given CategoryDefinition is within this GameItemDefinition.
        /// </summary>
        /// <param name="category">The Category to search for.</param>
        /// <returns>Whether or not this GameItemDefinition has the specified CategoryDefinition included.</returns>
        public bool HasCategoryDefinition(CategoryDefinition category)
        {
            if (category == null)
            {
                return false;
            }

            foreach (int currentCategoryHash in m_Categories)
            {
                if (currentCategoryHash == category.hash)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called before serialization, this will copy over all keys and values from 
        /// the DetailDefinitions dictionary into their serializable lists.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_DetailDefinitionValues.Clear();
            
            foreach (var kv_Detail in m_DetailDefinitions)
            {
                m_DetailDefinitionValues.Add(kv_Detail.Value);
            }
        }

        /// <summary>
        /// Called after serialization, this will pull out the DetailDefinition keys and values from the lists and store them into the main dictionary.
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_DetailDefinitions = new Dictionary<Type, BaseDetailDefinition>();

            for (int i = 0; i < m_DetailDefinitionValues.Count; i++)
            {
                m_DetailDefinitions.Add(m_DetailDefinitionValues[i].GetType(), m_DetailDefinitionValues[i]);
            }
        }

        /// <summary>
        /// Creates a new GameItemDefinition by Id and displayName.
        /// </summary>
        /// <param name="id">The Id this GameItemDefinition will use.</param>
        /// <param name="displayName">The display name this GameItemDefinition will use.</param>
        /// <returns>The newly created GameItemDefinition.</returns>
        public static GameItemDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot create a GameItemDefinition while in play mode.");

            GameItemDefinition gameItem = CreateInstance<GameItemDefinition>();
            gameItem.Initialize(id, displayName);
            gameItem.name = $"{id}_GameItem";

            return gameItem;
        }

        protected virtual void Initialize(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("GameItemDefinition cannot have null or empty id.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("GameItemDefinition can only be alphanumeric with optional dashes or underscores.");
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new System.ArgumentException("GameItemDefinition cannot have null or empty displayName.");
            }

            m_Id = id;
            m_DisplayName = displayName;
            m_Hash = Tools.StringToHash(id);
        }
    }
}
