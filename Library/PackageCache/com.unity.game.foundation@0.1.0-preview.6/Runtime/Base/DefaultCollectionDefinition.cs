using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// DefaultCollectionDefinitions contain preset values and rules for a 
    /// Collection by using a CollectionDefinition. During runtime, it may 
    /// be useful to refer back to the DefaultCollectionDefinition for the 
    /// presets and rules, but the values cannot be changed at runtime (your 
    /// system may, for example, bypass the presets, or calculate new values 
    /// on the fly with modifiers).
    /// </summary>
    [System.Serializable]
    public class DefaultCollectionDefinition
    {
        public DefaultCollectionDefinition(string id, string displayName, int baseCollectionDefinitionHash)
        {
            Tools.ThrowIfPlayMode("Cannot create a DefaultCollectionDefinition while in play mode.");

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("DefaultCollectionDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            m_DisplayName = displayName;
            m_Id = id;
            m_Hash = Tools.StringToHash(m_Id);
            m_CollectionDefinitionHash = baseCollectionDefinitionHash;
        }

        [SerializeField] 
        private string m_Id;

        /// <summary>
        /// The string id of this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>The string Id of this DefaultCollectionDefinition.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField] 
        private int m_Hash;

        /// <summary>
        /// The Hash of this DefaultCollectionDefinition's Id.
        /// </summary>
        /// <returns>The Hash of this DefaultCollectionDefinition's Id.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField] 
        private string m_DisplayName;

        /// <summary>
        /// The name of this DefaultCollectionDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this DefaultCollectionDefinition for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { SetDisplayName(value); }
        }

        [SerializeField]
        private int m_CollectionDefinitionHash;

        /// <summary>
        /// CollectionDefinition used for this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>CollectionDefinition used for this DefaultCollectionDefinition.</returns>
        public int collectionDefinitionHash
        {
            get { return m_CollectionDefinitionHash; }
        }

        private void SetDisplayName(string name)
        {
            Tools.ThrowIfPlayMode("Cannot set the display name of a DefaultCollectionDefinition while in play mode.");

            m_DisplayName = name;
        }
    }
}
