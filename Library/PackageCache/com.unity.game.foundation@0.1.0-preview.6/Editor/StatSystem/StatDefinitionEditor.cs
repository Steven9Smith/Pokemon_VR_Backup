using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Editor tab for the Stat window that allows creation of Stat definitions.
    /// </summary>
    internal class StatDefinitionEditor : CollectionEditorBase<StatDefinition>
    {
        private string m_CurrentItemId = null;
        private int m_SelectedTypePopupIdx = 0;
        private string[] m_TypeNamesForPopup = System.Enum.GetNames( typeof( StatDefinition.StatValueType ) );
        private StatDefinition.StatValueType m_NewItemValueType;

        protected override List<StatDefinition> m_Items
        {
            get { return EditorAPIHelper.GetStatCatalogDefinitionsList(); }
        }

         protected override List<StatDefinition> m_FilteredItems
        {
            get { return m_Items; }
        }

        /// <summary>
        /// Constructor for the StatDefinitionEditor class.
        /// </summary>
        public StatDefinitionEditor(string name, StatEditorWindow window) : base(name, window)
        {
        }

        /// <summary>
        /// Override base class method for what happens when the tab is opened.
        /// </summary>
        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database.statCatalog == null) return;

             SelectFilteredItem(0); // Select the first Item
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(m_Items.Select(i => i.id)));
            m_SelectedTypePopupIdx = 0;
            m_NewItemValueType = StatDefinition.StatValueType.Int;
        }

        protected override void DrawCreateInputFields()
        {
            base.DrawCreateInputFields();
            DrawValueTypePopup();
        }

        protected override void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox("Once the Create button is clicked Id and Value Type cannot be changed.", MessageType.Warning);
        }

        protected void DrawValueTypePopup()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Value Type", GUILayout.Width(145));
                int newFilterIdx = EditorGUILayout.Popup(m_SelectedTypePopupIdx, m_TypeNamesForPopup);
                if (newFilterIdx != m_SelectedTypePopupIdx)
                {
                    m_SelectedTypePopupIdx = newFilterIdx;
                    Enum.TryParse(m_TypeNamesForPopup[m_SelectedTypePopupIdx], out m_NewItemValueType);
                }
            }
        }

        protected override void CreateNewItemFinalize()
        {
            StatDefinition itemDefinition = EditorAPIHelper.CreateStatDefinition(m_NewItemId, m_NewItemDisplayName, m_NewItemValueType);

            AddItem(itemDefinition);
            SelectItem(itemDefinition);
            DrawDetail(itemDefinition, m_Items.FindIndex(x => x.Equals(m_SelectedItem)), m_Items.Count);
        }

        protected override void AddItem(StatDefinition item)
        {
            EditorAPIHelper.AddStatDefinitionToStatCatalog(item);
            EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
            window.Repaint();
        }

        protected override void DrawDetail(StatDefinition item, int index, int count)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = item.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (item.displayName != displayName)
                {
                    item.displayName = displayName;
                    EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Value Type", GUILayout.Width(145));
                    EditorGUILayout.SelectableLabel(item.statValueType.ToString(), GUILayout.Height(15), GUILayout.ExpandWidth(true));
                }
            }
        }

        protected override void DrawSidebarListItem(StatDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(220f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 220, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(StatDefinition item)
        {
            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(m_Items.Select(i => i.id)));
                m_CurrentItemId = item.id;
                m_SelectedTypePopupIdx = (int)item.statValueType;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(StatDefinition item)
        {
            // If a Stat item is deleted, remove it from list
            if (item != null)
            {
                EditorAPIHelper.RemoveStatDefinitionFromStatCatalog(item);
                EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
            }
        }
    }
}
