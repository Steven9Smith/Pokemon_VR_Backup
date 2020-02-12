using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class CategoryDefinitionEditor : CollectionEditorBase<CategoryDefinition>
    {
        private string m_CurrentCategoryDefinitionId = null;

        protected override List<CategoryDefinition> m_Items
        {
            get
            {
                return EditorAPIHelper.GetInventoryCatalogCategoriesList();
            }
        }

        protected override List<CategoryDefinition> m_FilteredItems
        {
            get
            { return m_Items; }
        }

        public CategoryDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            var itemCatalogDatabase = GameFoundationSettings.database.inventoryCatalog;
            if (itemCatalogDatabase == null)
                return;

            SelectFilteredItem(0); // Select the first Item
        }

        protected override void SelectItem(CategoryDefinition categoryDefinition)
        {
            if (categoryDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(m_Items.Select(i => i.id)));
                m_CurrentCategoryDefinitionId = categoryDefinition.id;
            }

            base.SelectItem(categoryDefinition);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(m_Items.Select(i => i.id)));
        }

        protected override void AddItem(CategoryDefinition category)
        {
            EditorAPIHelper.AddCategoryDefinitionToInventoryCatalog(category);
            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            window.Repaint();
        }

        protected override void CreateNewItemFinalize()
        {
            CategoryDefinition categoryDefinition = EditorAPIHelper.CreateCategoryDefinition(m_NewItemId, m_NewItemDisplayName);

            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            AddItem(categoryDefinition);
            SelectItem(categoryDefinition);
            m_CurrentCategoryDefinitionId = m_NewItemId;
            DrawDetail(categoryDefinition, m_Items.FindIndex(x => x.Equals(m_SelectedItem)), m_Items.Count);
        }

        protected override void DrawDetail(CategoryDefinition categoryDefinition, int index, int count)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = categoryDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentCategoryDefinitionId, ref displayName);

                if (categoryDefinition.displayName != displayName)
                {
                    categoryDefinition.displayName = displayName;
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }
            }
        }

        protected override void DrawSidebarListItem(CategoryDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void OnRemoveItem(CategoryDefinition item)
        {
            if (item != null)
            {
                // loop through all inventory items and attempt to remove category in case they are referring to it.
                foreach (InventoryItemDefinition inventoryItemDefinition in GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions())
                {
                    inventoryItemDefinition.RemoveCategory(item);
                }

                CategoryFilterEditor.ResetCategoryFilter();
                EditorAPIHelper.RemoveCategoryDefinitionFromInventoryCatalog(item);
            }
        }
    }
}
