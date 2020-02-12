using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a StatItem.
    /// </summary>
    [Serializable]
    public class StatItemSerializableData
    {
        [SerializeField] int m_GameItemId;
        [SerializeField] string m_DefinitionId;
        
        [SerializeField] int m_IntValue;
        [SerializeField] int m_DefaultIntValue;
        
        [SerializeField] float m_FloatValue;
        [SerializeField] float m_DefaultFloatValue;

        /// <summary>
        /// The GameItem hash id of the stat.
        /// </summary>
        public int gameItemId
        {
            get { return m_GameItemId; }
        }

        /// <summary>
        /// The definition id of the stat.
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }

        /// <summary>
        /// The int value of the stat when StatDictionary type is an int.
        /// </summary>
        public int intValue
        {
            get { return m_IntValue; }
        }

        /// <summary>
        /// The default int value of the stat when StatDictionary type is an int.
        /// </summary>
        public int defaultIntValue
        {
            get { return m_DefaultIntValue; }
        }
        
        /// <summary>
        /// The float value of the stat when StatDictionary type is a float.
        /// </summary>
        public float floatValue
        {
            get { return m_FloatValue; }
        }

        /// <summary>
        /// The float value of the stat when StatDictionary type is a float.
        /// </summary>
        public float defaultFloatValue
        {
            get { return m_DefaultFloatValue; }
        }

        /// <summary>
        /// Basic constructor that take in the type of the value of the stat, the GameItem hash id of the stat, the definition id of the stat, the current value and the default value of the stat.
        /// </summary>
        public StatItemSerializableData(StatDictionarySerializableData.StatType type, int gameItemId, string definitionId, object value, object defaultValue)
        {
            m_GameItemId = gameItemId;
            m_DefinitionId = definitionId;

            switch (type)
            {
                case StatDictionarySerializableData.StatType.Int:
                    m_IntValue = (int) value;
                    m_DefaultIntValue = (int) defaultValue;
                    break;
                
                case StatDictionarySerializableData.StatType.Float:
                    m_FloatValue = (float) value;
                    m_DefaultFloatValue = (float) defaultValue;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public StatItemSerializableData()
        {
        }
    }
}