namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is one entry in the list of possible stats an item could have.
    /// </summary>
    [System.Serializable]
    public class StatDefinition
    {
        /// <summary>
        /// Enum to determine the value type for this StatDefinition
        /// </summary>
        public enum StatValueType
        {
            Int,
            Float,
        }

        /// <summary>
        /// This is one entry in the list of possible Stats an item could have.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="id">The Id this StatDefinition will use.</param>
        /// <param name="displayName">The name this StatDefinition will use.</param>
        /// <param name="statValueType">The value type this StatDefinition will hold.</param>
        public StatDefinition(string id, string displayName, StatValueType statValueType)
        {
            Tools.ThrowIfPlayMode("Cannot construct a StatDefinition while in play mode.");

            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentNullException("StatDefinition cannot have null or empty ids.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("StatDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            m_Id = id;
            m_IdHash = Tools.StringToHash(id);
            m_DisplayName = displayName;
            m_StatValueType = statValueType;
        }

        /// <summary>
        /// Id for this Stat definition.
        /// </summary>
        /// <returns>id for this Stat definition.</returns>
        public string id
        {
            get { return m_Id; }
        }
        [SerializeField]
        private string m_Id;

        /// <summary>
        /// Hash for Id string for this Stat definition.
        /// </summary>
        /// <returns>Hash for Id string for this Stat definition.</returns>
        public int idHash
        {
            get { return m_IdHash; }
        }
        [SerializeField]
        private int m_IdHash;

        /// <summary>
        /// Custom string attached to this Stat definition.
        /// </summary>
        /// <returns>Custom string attached to this Stat definition.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }
        [SerializeField]
        private string m_DisplayName;

        /// <summary>
        /// Stat value type for this Stat definition.
        /// </summary>
        /// <returns>Stat value type for this Stat definition.</returns>
        public StatValueType statValueType
        {
            get { return m_StatValueType; }
        }
        [SerializeField]
        private StatValueType m_StatValueType;

        internal bool DoesValueTypeMatch(System.Type type)
        {
            switch (statValueType)
            {
                case StatValueType.Int:
                    return type == typeof(int);
                case StatValueType.Float:
                    return type == typeof(float);
                default:
                    throw new System.InvalidOperationException("Invalid type passed to DoesValueTypeMatch");
            }
        }
    }
}
