using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Detail definition to establish that item uses certain stats, and also to set default values for those stats.
    /// </summary>
    /// <inheritdoc/>
    public class StatDetailDefinition : BaseDetailDefinition, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Returns 'friendly' display name for this detail definition.
        /// </summary>
        public override string DisplayName() { return "Stat Detail"; }

        private Dictionary<int, int> m_StatDefaultIntValues;
        private Dictionary<int, float> m_StatDefaultFloatValues;

        internal Dictionary<int, int> statDefaultIntValues
        {
            get { return m_StatDefaultIntValues; }
        }

        internal Dictionary<int, float> statDefaultFloatValues
        {
            get { return m_StatDefaultFloatValues; }
        }

        // note: these internal lists are used to serialize and deserialize the above dictionary
        [SerializeField, HideInInspector]
        private List<int> m_StatDefaultIntValues_Keys;

        [SerializeField, HideInInspector]
        private List<int> m_StatDefaultIntValues_Values;

        [SerializeField, HideInInspector]
        private List<int> m_StatDefaultFloatValues_Keys;

        [SerializeField, HideInInspector]
        private List<float> m_StatDefaultFloatValues_Values;

        // internal constructor to prohibit developers instantiating StatDetailDefinitions.
        internal StatDetailDefinition()
        {
        }

#if UNITY_EDITOR
        private void HandleStatCatalogWillRemoveStatDefinition(object sender, StatDefinition statDefinition)
        {
            switch (statDefinition.statValueType)
            {
                case StatDefinition.StatValueType.Int:
                    RemoveStatInt(statDefinition.idHash);
                    EditorUtility.SetDirty(this);
                    return;

                case StatDefinition.StatValueType.Float:
                    RemoveStatFloat(statDefinition.idHash);
                    EditorUtility.SetDirty(this);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(statDefinition), "statDefinition has an unrecognized StatValueType");
            }
        }

        private void OnEnable()
        {
            StatCatalog.OnWillRemoveStatDefinition += HandleStatCatalogWillRemoveStatDefinition;
        }

        private void OnDisable()
        {
            StatCatalog.OnWillRemoveStatDefinition -= HandleStatCatalogWillRemoveStatDefinition;
        }
#endif

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(string statDefinitionId)
        {
            return GetStatInt(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionIdHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(int statDefinitionIdHash)
        {
            if (m_StatDefaultIntValues != null)
            {
                if (m_StatDefaultIntValues != null && m_StatDefaultIntValues.TryGetValue(statDefinitionIdHash, out int value))
                {
                    return value;
                }
            }
            throw new KeyNotFoundException("Attempted to get stat default int value for stat that does not exist.");
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(string statDefinitionId)
        {
            return GetStatFloat(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(int statDefinitionHash)
        {
            if (m_StatDefaultFloatValues != null)
            {
                if (m_StatDefaultFloatValues.TryGetValue(statDefinitionHash, out float value))
                {
                    return value;
                }
            }
            throw new KeyNotFoundException("Attempted to get stat default float value for stat that does not exist.");
        }

        /// <summary>
        /// Adds default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <param name="value">The default value for the stat.</param>
        public void AddStatInt(int statDefinitionHash, int value)
        {
            Tools.ThrowIfPlayMode("Cannot Add Stat Default Int Value while in play mode.");

            if (m_StatDefaultIntValues == null)
            {
                m_StatDefaultIntValues = new Dictionary<int, int>();
            }
            m_StatDefaultIntValues[statDefinitionHash] = value;
        }

        /// <summary>
        /// Adds default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <param name="value">The default value for the stat.</param>
        public void AddStatFloat(int statDefinitionHash, float value)
        {
            Tools.ThrowIfPlayMode("Cannot Add Stat Default Float Value while in play mode.");

            if (m_StatDefaultFloatValues == null)
            {
                m_StatDefaultFloatValues = new Dictionary<int, float>();
            }
            m_StatDefaultFloatValues[statDefinitionHash] = value;
        }

        /// <summary>
        /// Remove default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>True if the specified default stat was removed.</returns>
        public bool RemoveStatInt(int statDefinitionHash)
        {
            Tools.ThrowIfPlayMode("Cannot Remove Stat Default Int Value while in play mode.");

            if (m_StatDefaultIntValues == null)
            {
                return false;
            }
            return m_StatDefaultIntValues.Remove(statDefinitionHash);
        }

        /// <summary>
        /// Remove default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>True if the specified default stat was removed.</returns>
        public bool RemoveStatFloat(int statDefinitionHash)
        {
            Tools.ThrowIfPlayMode("Cannot Remove Stat Default Float Value while in play mode.");

            if (m_StatDefaultFloatValues == null)
            {
                return false;
            }
            return m_StatDefaultFloatValues.Remove(statDefinitionHash);
        }

        /// <summary>
        /// Called before serializing to prepare dictionary to be serialized
        /// </summary>
        public void OnBeforeSerialize()
        {
            CopyDictionaryToLists<int>(m_StatDefaultIntValues, out m_StatDefaultIntValues_Keys, out m_StatDefaultIntValues_Values);
            CopyDictionaryToLists<float>(m_StatDefaultFloatValues, out m_StatDefaultFloatValues_Keys, out m_StatDefaultFloatValues_Values);
        }

        /// <summary>
        /// Called after deserializing to update dictionary from serialized data
        /// </summary>
        public void OnAfterDeserialize()
        {
            CopyListsToDictionary<int>(m_StatDefaultIntValues_Keys, m_StatDefaultIntValues_Values, ref m_StatDefaultIntValues);
            CopyListsToDictionary<float>(m_StatDefaultFloatValues_Keys, m_StatDefaultFloatValues_Values, ref m_StatDefaultFloatValues);
        }

        private static void CopyDictionaryToLists<T>(Dictionary<int, T> dictionary, out List<int> keys, out List<T> values)
        {
            if (dictionary == null || dictionary.Count <= 0)
            {
                keys = null;
                values = null;
            }
            else
            {
                int count = dictionary.Count;

                keys = new List<int>(count);
                values = new List<T>(count);

                foreach (var kv in dictionary)
                {
                    keys.Add(kv.Key);
                    values.Add(kv.Value);
                }
            }
        }

        private static void CopyListsToDictionary<T>(List<int> keys, List<T> values, ref Dictionary<int, T> dictionary)
        {
            if (keys == null || values == null)
            {
                dictionary = null;
            }
            else
            {
                int count = Math.Min(keys.Count, values.Count);
                if (count <= 0)
                {
                    dictionary = null;
                }
                else
                {
                    if (dictionary == null)
                    {
                        dictionary = new Dictionary<int, T>();
                    }
                    else
                    {
                        dictionary.Clear();
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        dictionary.Add(keys[i], values[i]);
                    }
                }
            }
        }
    }
}
