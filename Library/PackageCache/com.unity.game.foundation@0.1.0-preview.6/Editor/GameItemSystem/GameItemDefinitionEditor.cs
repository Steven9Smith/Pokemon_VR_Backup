using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class GameItemDefinitionEditor : CollectionEditorBase<GameItemDefinition>
    {
        private string m_CurrentItemId = null;

        protected override List<GameItemDefinition> m_Items
        {
            get
            {
                return EditorAPIHelper.GetGameItemCatalogGameItemDefinitionsList();
            }
        }

        protected override List<GameItemDefinition> m_FilteredItems
        {
            get
            {
                return m_Items;
            }
        }

        public GameItemDefinitionEditor(string name, GameItemEditorWindow window) : base(name, window)
        {
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database.gameItemCatalog == null) return;

            SelectFilteredItem(0); // Select the first Item
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(m_Items.Select(i => i.id)));
        }

        protected override void CreateNewItemFinalize()
        {
            GameItemDefinition gameItemDefinition = GameItemDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(gameItemDefinition, GameFoundationSettings.database.gameItemCatalog);

            EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);

            AddItem(gameItemDefinition);
            SelectItem(gameItemDefinition);
            m_CurrentItemId = m_NewItemId;
            DrawGeneralDetail(gameItemDefinition);
        }

        protected override void AddItem(GameItemDefinition item)
        {
            EditorAPIHelper.AddGameItemDefinitionToGameItemCatalog(item);
            EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);
            window.Repaint();
        }

        protected override void DrawDetail(GameItemDefinition gameItemDefinition, int index, int count)
        {
            DrawGeneralDetail(gameItemDefinition);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(gameItemDefinition);
        }

        private void DrawGeneralDetail(GameItemDefinition gameItemDefinition)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = gameItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (gameItemDefinition.displayName != displayName)
                {
                    gameItemDefinition.displayName = displayName;
                    EditorUtility.SetDirty(gameItemDefinition);
                }
            }
        }

        protected override void DrawSidebarListItem(GameItemDefinition gameItemDefinition, int index)
        {
            BeginSidebarItem(gameItemDefinition, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(gameItemDefinition.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(gameItemDefinition);

            EndSidebarItem(gameItemDefinition, index);
        }

        protected override void SelectItem(GameItemDefinition item)
        {
            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(m_Items.Select(i => i.id)));
                m_CurrentItemId = item.id;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(GameItemDefinition item)
        {
            if (item != null)
            {
                // If a GameItem item is deleted, handling removing its asset as well
                CollectionEditorTools.AssetDatabaseRemoveObject(item);
                EditorAPIHelper.RemoveGameItemDefinitionFromGameItemCatalog(item);
                EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);
            }
        }
    }
}
