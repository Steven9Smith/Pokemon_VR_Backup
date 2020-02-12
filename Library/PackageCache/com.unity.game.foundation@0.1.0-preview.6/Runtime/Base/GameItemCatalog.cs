using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A Catalog for all GameItemDefinitions.
    /// </summary>
    public class GameItemCatalog : ScriptableObject
    {
        internal static event System.EventHandler<GameItemDefinition> OnWillRemoveGameItemDefinition;

        [SerializeField]
        private List<CategoryDefinition> m_Categories = new List<CategoryDefinition>();

        /// <summary>
        /// A dictionary of all CategoryDefinitions.
        /// </summary>
        protected internal List<CategoryDefinition> categories
        {
            get => m_Categories;
            set => m_Categories = value;
        }
        
        [SerializeField]
        private List<GameItemDefinition> m_Definitions = new List<GameItemDefinition>();

        /// <summary>
        /// A dictionary of all GameItemDefinitions.
        /// </summary>
        protected internal List<GameItemDefinition> definitions
        {
            get => m_Definitions;
            set => m_Definitions = value;
        }

        /// <summary>
        /// Returns the categories in this catalog in an array.
        /// </summary>
        /// <returns>The categories in this catalog in an array.</returns>
        public CategoryDefinition[] GetCategories()
        {
            if (m_Categories == null)
            {
                return null;
            }
            
            return m_Categories.ToArray();
        }

        /// <summary>
        /// Fills the given list with all categories found in this catalog.
        /// </summary>
        /// <param name="categories">The list to be filled up</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (m_Categories == null || categories == null)
            {
                return;
            }
            
            categories.AddRange(m_Categories);
        }

        /// <summary>
        /// Adds the given Category to this GameItemCatalog.
        /// </summary>
        /// <param name="category">The Category to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the given category is a duplicate.</exception>
        public bool AddCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot add a Category to a GameItemCatalog while in play mode.");

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
        /// Removes the given Category from this GameItemCatalog.
        /// </summary>
        /// <param name="category">The Category to remove</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot remove a Category from a GameItemCatalog while in play mode.");
            
            return m_Categories.Remove(category);
        }

        /// <summary>
        /// Returns an array of all game item definitions in this catalog.
        /// </summary>
        /// <returns>An array of all game item definitions in this catalog.</returns>
        public GameItemDefinition[] GetGameItemDefinitions()
        {
            if (m_Definitions == null)
            {
                return null;
            }
            
            return m_Definitions.ToArray();
        }

        /// <summary>
        /// Fills the given array with all game item definitions in this catalog.
        /// </summary>
        /// <param name="gameItems">The list to fill up.</param>
        public void GetGameItemDefinitions(List<GameItemDefinition> gameItems)
        {
            if (m_Definitions == null || gameItems == null)
            {
                return;
            }
            
            gameItems.AddRange(m_Definitions);
        }

        /// <summary>
        /// Adds the given GameItemDefinition to this GameItemCatalog.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition to add.</param>
        /// <returns>Whether or not the GameItemDefinition was successfully added.</returns>
        /// <exception cref="ArgumentException">Thrown if the given game item definition is a duplicate.</exception>
        public bool AddGameItemDefinition(GameItemDefinition gameItemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a GameItemDefinition to a GameItemCatalog while in play mode.");

            if (gameItemDefinition == null)
            {
                return false;
            }

            if (GetGameItemDefinition(gameItemDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + gameItemDefinition.id + ", hash: " + gameItemDefinition.hash + ")");
            }
            
            m_Definitions.Add(gameItemDefinition);
            return true;
        }

        /// <summary>
        /// Removes the given GameItemDefinition from this GameItemCatalog.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition to remove.</param>
        /// <returns>Whether or not the GameItemDefinition was successfully removed.</returns>
        public bool RemoveGameItemDefinition(GameItemDefinition gameItemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a GameItemDefinition from a GameItemCatalog while in play mode.");

            if (gameItemDefinition == null)
            {
                return false;
            }

            if (!m_Definitions.Contains(gameItemDefinition))
            {
                return false;
            }

            // this tells the GameItemDefinition to clean itself up (remove its child assets from the catalog asset, etc.)
            // this is different from telling all event subscribers to clean up whatever they want to clean up
            gameItemDefinition.OnRemove();

            // Now tell all event subscribers that the delete is happening to this definition.
            // The listening objects can do whatever they want with this information.
            // For example, if an InventoryItemDefinition has this GameItemDefinition set as its ReferenceDefinition,
            // then the InventoryItemDefinition listens to this event
            // and knows when to set its ReferenceDefinition field to null.
            OnWillRemoveGameItemDefinition?.Invoke(this, gameItemDefinition);

            return m_Definitions.Remove(gameItemDefinition);
        }

        /// <summary>
        /// Return specified GameItemDefinition by GameItemDefinition id string.
        /// </summary>
        /// <param name="gameItemDefinitionId">The GameItemDefinition Id string to find.</param>
        /// <returns>Specified GameItemDefinition in this GameItemCatalog.</returns>
        public GameItemDefinition GetGameItemDefinition(string gameItemDefinitionId)
        {
            if (string.IsNullOrEmpty(gameItemDefinitionId))
            {
                return null;
            }
            
            return GetGameItemDefinition(Tools.StringToHash(gameItemDefinitionId));
        }

        /// <summary>
        /// Return specified GameItemDefinition by Hash.
        /// </summary>
        /// <param name="gameItemDefinitionHash">The Hash of the GameItemDefinition to find.</param>
        /// <returns>Specified GameItemDefinition.</returns>
        public GameItemDefinition GetGameItemDefinition(int gameItemDefinitionHash)
        {
            foreach(var definition in m_Definitions)
            {
                if (definition.hash == gameItemDefinitionHash)
                {
                    return definition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns specified Category by CategoryDefinition Hash.
        /// </summary>
        /// <param name="categoryId">The Id of the Id of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid Hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if not found.</returns>
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
        /// This will return an array of GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryId">The id string of the Category we want to iterate.</param>
        /// <returns>An array of GameItemDefinitions that contain the given Category.</returns>
        public GameItemDefinition[] GetDefinitionsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            List<GameItemDefinition> gameItemDefinitions = new List<GameItemDefinition>();
            GetDefinitionsByCategory(categoryId, gameItemDefinitions);

            return gameItemDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with the GameItemDefinitions that have the designated category.
        /// </summary>
        /// <param name="categoryId">The id string of the Category we want to iterate.</param>
        /// <param name="gameItemDefinitions">The list to fill up.</param>
        public void GetDefinitionsByCategory(string categoryId, List<GameItemDefinition> gameItemDefinitions)
        {
            if (string.IsNullOrEmpty(categoryId) || gameItemDefinitions == null)
            {
                return;
            }

            GetDefinitionsByCategory(Tools.StringToHash(categoryId), gameItemDefinitions);
        }

        /// <summary>
        /// This will return an array of GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we want to iterate.</param>
        /// <returns>An array of GameItemDefinitions that contain the requested Category.</returns>
        public GameItemDefinition[] GetDefinitionsByCategory(int categoryHash)
        {
            if (m_Definitions == null)
            {
                return null;
            }

            List<GameItemDefinition> gameItemDefinitions = new List<GameItemDefinition>();
            GetDefinitionsByCategory(categoryHash, gameItemDefinitions);

            return gameItemDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with the GameItemDefinitions that have the designated category.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we want to iterate.</param>
        /// <param name="gameItemDefinitions">The list to fill up.</param>
        public void GetDefinitionsByCategory(int categoryHash, List<GameItemDefinition> gameItemDefinitions)
        {
            if (m_Definitions == null)
            {
                return;
            }

            foreach (var definition in m_Definitions)
            {
                foreach (var category in definition.GetCategories())
                {
                    if (category.hash == categoryHash)
                    {
                        gameItemDefinitions.Add(definition);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the given Hash is not yet included in this GameItemCatalog's list of GameItemDefinitions and is available for use.
        /// </summary>
        /// <param name="gameItemDefinitionId">The Hash to search for in this Catalog's GameItemDefinitions list.</param>
        /// <returns>True/False whether or not Hash is available for use.</returns>
        public bool IsDefinitionHashUnique(string gameItemDefinitionId)
        {
            if (string.IsNullOrEmpty(gameItemDefinitionId))
            {
                return false;
            }
            
            return GetGameItemDefinition(Tools.StringToHash(gameItemDefinitionId)) == null;
        }

        /// <summary>
        /// Simple factory method for creating an empty GameItemCatalog.
        /// </summary>
        /// <returns>The newly created GameItemCatalog.</returns>
        public static GameItemCatalog Create()
        {
            Tools.ThrowIfPlayMode("Cannot create a GameItem Catalog while in play mode.");
            
            var catalog = ScriptableObject.CreateInstance<GameItemCatalog>();

            return catalog;
        }
    }
}
