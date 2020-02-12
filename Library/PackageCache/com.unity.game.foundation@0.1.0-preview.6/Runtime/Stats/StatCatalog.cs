using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for StatDefinitions.
    /// The Stat Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    public class StatCatalog : ScriptableObject
    {
        internal static event System.EventHandler<StatDefinition> OnWillRemoveStatDefinition;

        [SerializeField]
        internal List<StatDefinition> m_StatDefinitions = new List<StatDefinition>();

        /// <summary>
        /// Returns an array of all stat definitions in this catalog.
        /// </summary>
        /// <returns>An array of all stat definitions in this catalog.</returns>
        public StatDefinition[] GetStatDefinitions()
        {
            if (m_StatDefinitions == null)
                return null;

            return m_StatDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with all stat definitions in this catalog.
        /// </summary>
        /// <param name="statDefinitions">The list to fill up.</param>
        public void GetStatDefinitions(List<StatDefinition> statDefinitions)
        {
            if (m_StatDefinitions == null || statDefinitions == null)
                return;
            
            statDefinitions.AddRange(m_StatDefinitions);
        }

        internal StatCatalog()
        {
        }

        /// <summary>
        /// Creates a new StatCatalog.
        /// </summary>
        /// <returns>Reference to the newly made StatCatalog.</returns>
        public static StatCatalog Create()
        {
            Tools.ThrowIfPlayMode("Cannot create a StatCatalog while in play mode.");

            var statCatalog = ScriptableObject.CreateInstance<StatCatalog>();

            return statCatalog;
        }

        /// <summary>
        /// Find and return a StatDefinition by its Id.
        /// </summary>
        /// <param name="statDefinitionId">Id of Stat Definition we're looking for.</param>
        /// <returns>StatDefinition for specified Stat Id</returns>
        public StatDefinition GetStatDefinition(string statDefinitionId)
        {
            if (string.IsNullOrEmpty(statDefinitionId))
            {
                return null;
            }
            return GetStatDefinition(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Find and return Stat definition by its Hash.
        /// </summary>
        /// <param name="statDefinitionHash"> Hash of Stat Definition we're looking for.</param>
        /// <returns>StatDefinition for specified Stat Hash </returns>
        public StatDefinition GetStatDefinition(int statDefinitionHash)
        {
            foreach(var definition in m_StatDefinitions)
            {
                if (definition.idHash == statDefinitionHash)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Determine if specified Stat definition Hash is unique in the Stat catalog.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to check.</param>
        /// <returns>True if Stat definition Hash is unique.</returns>
        public bool IsStatDefinitionHashUnique(int statDefinitionHash)
        {
            foreach (var definition in m_StatDefinitions)
            {
                if (definition.idHash == statDefinitionHash)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Adds the given StatDefintion to this Catalog.
        /// </summary>
        /// <param name="statDefinition">The StatDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public bool AddStatDefinition(StatDefinition statDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a StatDefinition to a Catalog while in play mode.");

            if (statDefinition == null)
                return false;

            if (m_StatDefinitions.Contains(statDefinition))
            {
                Debug.LogWarning("The object is already registered within this Stat Catalog. (id: " + statDefinition.id + ", hash: " + statDefinition.idHash + ")");
                return false;
            }

            if (GetStatDefinition(statDefinition.idHash) != null)
            {
                Debug.LogWarning("The hash for this object is already registered within this Stat Catalog. (id: " + statDefinition.id + ", hash: " + statDefinition.idHash + ")");
                return false;
            }

            m_StatDefinitions.Add(statDefinition);
            return true;
        }

        /// <summary>
        /// Removes the given StatDefinition from this Catalog.
        /// </summary>
        /// <param name="statDefinition">The StatDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveStatDefinition(StatDefinition statDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a StatDefinition from a Catalog while in play mode.");

            OnWillRemoveStatDefinition?.Invoke(this, statDefinition);

            return m_StatDefinitions.Remove(statDefinition);
        }
    }
}
