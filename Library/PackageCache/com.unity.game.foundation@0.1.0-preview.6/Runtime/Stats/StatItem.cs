namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is one record in the Stat Manager’s list of current Stats at runtime.
    /// </summary>
    public class StatItem<T> where T : new()
    {
        /// <summary>
        /// Constructs a new Stat item with desired values, using default value.
        /// </summary>
        /// <param name="gameItemId">GameItem's Id to apply Stat to.</param>
        /// <param name="definitionId">StatDefinition's Id for Stat to set.</param>
        internal StatItem(int gameItemId, string definitionId)
        {
            m_GameItemId = gameItemId;
            m_DefinitionId = definitionId;
            m_Value = new T();
            m_DefaultValue = new T();
        }

        /// <summary>
        /// Constructs a new Stat item with desired values, including initial Stat value.
        /// </summary>
        /// <param name="gameItemId">GameItem's Id to apply Stat to.</param>
        /// <param name="definitionId">StatDefinition's Id for Stat to set.</param>
        /// <param name="value">Current and default value for this Stat.</param>
        internal StatItem(int gameItemId, string definitionId, T value)
        {
            m_GameItemId = gameItemId;
            m_DefinitionId = definitionId;
            m_Value = value;
            m_DefaultValue = value;
        }
        
        /// <summary>
        /// Constructs a new stat item with current and default value..
        /// </summary>
        /// <param name="gameItemId">GameItem's id to apply stat to.</param>
        /// <param name="definitionId">StatDefinition's id for stat to set.</param>
        /// <param name="value">Current value for this stat.</param>
        /// <param name="defaultValue">Default value for this stat.</param>
        internal StatItem(int gameItemId, string definitionId, T value, T defaultValue)
        {
            m_GameItemId = gameItemId;
            m_DefinitionId = definitionId;
            m_Value = value;
            m_DefaultValue = defaultValue;
        }

        /// <summary>
        ///  Hash  of gameItem and is the same Id as the first key of Dictionary in the StatManager.
        /// </summary>
        /// <returns> Hash  of gameItem and is the same Id as the first key of Dictionary in the StatManager.</returns>
        public int gameItemId
        {
            get { return m_GameItemId; }
        }
        private int m_GameItemId;

        /// <summary>
        /// Stat definition Id string.
        /// </summary>
        /// <returns>Stat definition Id string.</returns>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }
        private string m_DefinitionId;

        /// <summary>
        /// Current Stat value.
        /// </summary>
        /// <returns>Current Stat value.</returns>
        public T value
        {
            get { return m_Value; }
            internal set { m_Value = value; }
        }
        private T m_Value;

        /// <summary>
        /// Default (initial) value for this Stat to allow resetting as needed.
        /// </summary>
        /// <returns>Default (initial) value for this Stat to allow resetting as needed.</returns>
        public T defaultValue
        {
            get { return m_DefaultValue; }
        }
        private T m_DefaultValue;
    }
}
