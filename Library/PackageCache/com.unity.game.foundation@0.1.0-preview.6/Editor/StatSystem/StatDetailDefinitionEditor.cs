using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(StatDetailDefinition))]
    internal class StatDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        private SerializedProperty m_StatDefaultIntValues_Keys_SerializedProperty;
        private SerializedProperty m_StatDefaultIntValues_Values_SerializedProperty;
        private SerializedProperty m_StatDefaultFloatValues_Keys_SerializedProperty;
        private SerializedProperty m_StatDefaultFloatValues_Values_SerializedProperty;
        private List<StatDefaultsListItem> m_ListItems = new List<StatDefaultsListItem>();
        private List<StatDefinition> m_AvailableStatDefinitions = new List<StatDefinition>();
        private string[] m_AvailableStatDefinitionDisplayNames;

        // These won't "stick" unless they're static.
        // That unfortunately means that we need to be careful with them
        // since multiple editor instances might try and access the same static field.
        private static int s_NewStatDefinitionSelectedIndex;
        private static StatDefinition s_StatDefinitionJustAdded;

        private void OnEnable()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets == null || targets.Length <= 0 || targets[0] == null)
            {
                return;
            }

            m_StatDefaultIntValues_Keys_SerializedProperty = serializedObject.FindProperty("m_StatDefaultIntValues_Keys");
            m_StatDefaultIntValues_Values_SerializedProperty = serializedObject.FindProperty("m_StatDefaultIntValues_Values");

            m_StatDefaultFloatValues_Keys_SerializedProperty = serializedObject.FindProperty("m_StatDefaultFloatValues_Keys");
            m_StatDefaultFloatValues_Values_SerializedProperty = serializedObject.FindProperty("m_StatDefaultFloatValues_Values");

            RefreshStatDefaultsCache();
        }

        /// <summary>
        /// Combine the lists and cache the merged collection in a static field.
        /// </summary>
        private void RefreshStatDefaultsCache()
        {
            m_ListItems.Clear();

            if (GameFoundationSettings.database.statCatalog == null)
            {
                return;
            }

            m_AvailableStatDefinitions.Clear();
            m_AvailableStatDefinitions.InsertRange(0, GameFoundationSettings.database.statCatalog.GetStatDefinitions());

            // INT

            for (int i = 0; i < m_StatDefaultIntValues_Keys_SerializedProperty.arraySize; i++)
            {
                int idHash = m_StatDefaultIntValues_Keys_SerializedProperty.GetArrayElementAtIndex(i).intValue;

                StatDefinition statDef = GameFoundationSettings.database.statCatalog.GetStatDefinition(idHash);

                if (statDef != null)
                {
                    m_ListItems.Add(new StatDefaultsListItem(i, statDef, m_StatDefaultIntValues_Values_SerializedProperty.GetArrayElementAtIndex(i)));
                    m_AvailableStatDefinitions.Remove(statDef);
                }
            }

            // FLOAT

            for (int i = 0; i < m_StatDefaultFloatValues_Keys_SerializedProperty.arraySize; i++)
            {
                int idHash = m_StatDefaultFloatValues_Keys_SerializedProperty.GetArrayElementAtIndex(i).intValue;

                StatDefinition statDef = GameFoundationSettings.database.statCatalog.GetStatDefinition(idHash);

                if (statDef != null)
                {
                    m_ListItems.Add(new StatDefaultsListItem(i, statDef, m_StatDefaultFloatValues_Values_SerializedProperty.GetArrayElementAtIndex(i)));
                    m_AvailableStatDefinitions.Remove(statDef);
                }
            }

            // sort the lists by display name
            // (sort the list items descending because that list is going to be iterated in reverse)

            m_AvailableStatDefinitions = m_AvailableStatDefinitions.OrderBy(statDef => statDef.displayName).ToList();
            m_ListItems = m_ListItems.OrderByDescending(listItem => listItem.statDefinition.displayName).ToList();

            // list of display names (indices matching those in the original list)

            m_AvailableStatDefinitionDisplayNames = new string[m_AvailableStatDefinitions.Count + 1];

            m_AvailableStatDefinitionDisplayNames[0] = "Select Stat";

            for (int i = 0; i < m_AvailableStatDefinitions.Count; i++)
            {
                m_AvailableStatDefinitionDisplayNames[i + 1] = m_AvailableStatDefinitions[i]?.displayName;
            }

            if (s_NewStatDefinitionSelectedIndex > m_AvailableStatDefinitionDisplayNames.Length - 1)
            {
                s_NewStatDefinitionSelectedIndex = 0;
            }
        }

        public override void OnInspectorGUI()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets == null || targets.Length <= 0 || targets[0] == null)
            {
                return;
            }

            if (GameFoundationSettings.database.statCatalog == null)
            {
                EditorGUILayout.HelpBox("No stat catalog found. Open the Game Foundation > Stat window to create one.", MessageType.Warning);

                return;
            }

            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    GUILayout.Label("Stat", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(130f));
                    GUILayout.Label("Type", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Label("Default Value", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(130f));
                    GUILayout.Space(40f); // "x" column
                }

                if (m_ListItems.Count > 0)
                {
                    for (int i = m_ListItems.Count - 1; i >= 0; i--)
                    {
                        // draw row: Stat display name | value type | default value | delete button

                        StatDefaultsListItem listItem = m_ListItems[i];

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(listItem.statDefinition.displayName, GUILayout.Width(140f));

                            GUILayout.Label(listItem.statDefinition.statValueType.ToString());

                            string controlName = $"valueField{listItem.GetHashCode().ToString()}";
                            GUI.SetNextControlName(controlName);
                            EditorGUILayout.PropertyField(listItem.valueProp, GUIContent.none, GUILayout.Width(130f));
                            if (ReferenceEquals(s_StatDefinitionJustAdded, listItem.statDefinition))
                            {
                                EditorGUI.FocusTextInControl(controlName);
                                s_StatDefinitionJustAdded = null;
                            }

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(40f)))
                            {
                                switch (listItem.statDefinition.statValueType)
                                {
                                    case StatDefinition.StatValueType.Int:
                                        m_StatDefaultIntValues_Keys_SerializedProperty.DeleteArrayElementAtIndex(listItem.indexInOriginalList);
                                        m_StatDefaultIntValues_Values_SerializedProperty.DeleteArrayElementAtIndex(listItem.indexInOriginalList);
                                        break;

                                    case StatDefinition.StatValueType.Float:
                                        m_StatDefaultFloatValues_Keys_SerializedProperty.DeleteArrayElementAtIndex(listItem.indexInOriginalList);
                                        m_StatDefaultFloatValues_Values_SerializedProperty.DeleteArrayElementAtIndex(listItem.indexInOriginalList);
                                        break;

                                    default:
                                        Debug.LogError("invalid type detected when trying to delete a stat default value");
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("no stats configured", GameFoundationEditorStyles.centeredGrayLabel);
                    EditorGUILayout.Space();
                }

                // UI for adding a new Stat (if there are any left unused)

                s_StatDefinitionJustAdded = null;

                if (m_AvailableStatDefinitionDisplayNames.Length > 1)
                {
                    // horizontal rule

                    Rect lineRect1 = EditorGUILayout.GetControlRect(false, 1);
                    EditorGUI.DrawRect(lineRect1, EditorGUIUtility.isProSkin ? Color.black : Color.gray);

                    using (new GUILayout.HorizontalScope())
                    {
                        // popup with Stat types that are not already in the dict

                        s_NewStatDefinitionSelectedIndex = EditorGUILayout.Popup(s_NewStatDefinitionSelectedIndex, m_AvailableStatDefinitionDisplayNames, GUILayout.Width(140f));

                        StatDefinition selectedStatDefinition = null;

                        if (s_NewStatDefinitionSelectedIndex > 0)
                        {
                            selectedStatDefinition = m_AvailableStatDefinitions[s_NewStatDefinitionSelectedIndex - 1];
                            GUILayout.Label(selectedStatDefinition.statValueType.ToString(), GUILayout.Width(130f));
                        }
                        else
                        {
                            GUILayout.Label("", GUILayout.Width(130f));
                        }

                        // placeholder for value
                        GUILayout.FlexibleSpace();

                        if (selectedStatDefinition == null)
                        {
                            GUI.enabled = false;
                        }
                        if (GUILayout.Button("Add", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(40f)))
                        {
                            if (selectedStatDefinition != null)
                            {
                                switch (selectedStatDefinition.statValueType)
                                {
                                    case StatDefinition.StatValueType.Int:
                                        int newIntIndex = m_StatDefaultIntValues_Keys_SerializedProperty.arraySize;
                                        m_StatDefaultIntValues_Keys_SerializedProperty.InsertArrayElementAtIndex(newIntIndex);
                                        m_StatDefaultIntValues_Keys_SerializedProperty.GetArrayElementAtIndex(newIntIndex).intValue = selectedStatDefinition.idHash;
                                        m_StatDefaultIntValues_Values_SerializedProperty.InsertArrayElementAtIndex(newIntIndex);
                                        s_StatDefinitionJustAdded = selectedStatDefinition;
                                        break;

                                    case StatDefinition.StatValueType.Float:
                                        int newFloatIndex = m_StatDefaultFloatValues_Keys_SerializedProperty.arraySize;
                                        m_StatDefaultFloatValues_Keys_SerializedProperty.InsertArrayElementAtIndex(newFloatIndex);
                                        m_StatDefaultFloatValues_Keys_SerializedProperty.GetArrayElementAtIndex(newFloatIndex).intValue = selectedStatDefinition.idHash;
                                        m_StatDefaultFloatValues_Values_SerializedProperty.InsertArrayElementAtIndex(newFloatIndex);
                                        s_StatDefinitionJustAdded = selectedStatDefinition;
                                        break;

                                    default:
                                        Debug.LogError("invalid type detected when trying to add a stat default value");
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError("selected stat definition is null when trying to add a stat default value");
                            }

                            s_NewStatDefinitionSelectedIndex = 0;
                        }

                        GUI.enabled = true;
                    }
                }

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();

                    RefreshStatDefaultsCache();
                }
            }

            EditorGUILayout.Space();
        }

        private class StatDefaultsListItem
        {
            public int indexInOriginalList;
            public StatDefinition statDefinition;
            public SerializedProperty valueProp;

            private StatDefaultsListItem()
            {
            }

            public StatDefaultsListItem(int inIndexInOriginalList, StatDefinition inStatDefinition, SerializedProperty inValueProp)
            {
                indexInOriginalList = inIndexInOriginalList;
                statDefinition = inStatDefinition;
                valueProp = inValueProp;
            }
        }
    }
}
