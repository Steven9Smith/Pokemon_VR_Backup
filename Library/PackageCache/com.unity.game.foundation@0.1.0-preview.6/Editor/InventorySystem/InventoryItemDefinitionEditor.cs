using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryItemDefinitionEditor : CollectionEditorBase<InventoryItemDefinition>
    {
        private string m_CurrentItemId = null;

        private CategoryPickerEditor m_CategoryPicker;

        protected override List<InventoryItemDefinition> m_Items
        {
            get
            {
                return EditorAPIHelper.GetInventoryCatalogItemDefinitionsList();
            }
        }

        protected override List<InventoryItemDefinition> m_FilteredItems
        {
            get
            {
                return CategoryFilterEditor.GetFilteredItems(m_Items, EditorAPIHelper.GetInventoryCatalogCategoriesList());
            }
        }

        public InventoryItemDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
            m_CategoryPicker = new CategoryPickerEditor(GameFoundationSettings.database.inventoryCatalog);
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                return;
            }

            CategoryFilterEditor.RefreshSidebarCategoryFilterList(EditorAPIHelper.GetInventoryCatalogCategoriesList());

            SelectFilteredItem(0); // Select the first Item
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(m_Items.Select(i => i.id)));
        }

        protected override void CreateNewItemFinalize()
        {
            InventoryItemDefinition itemDefinition = EditorAPIHelper.CreateInventoryItemDefinition(m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(itemDefinition, GameFoundationSettings.database.inventoryCatalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            CategoryDefinition currentFilteredCategory = CategoryFilterEditor.GetCurrentFilteredCategory(EditorAPIHelper.GetInventoryCatalogCategoriesList());
            if (currentFilteredCategory != null)
            {
                List<CategoryDefinition> existingItemCategories = EditorAPIHelper.GetGameItemDefinitionCategories(itemDefinition);
                if (existingItemCategories != null && !existingItemCategories.Any(category => category.hash == currentFilteredCategory.hash))
                {
                    itemDefinition.AddCategory(currentFilteredCategory);
                }
            }

            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            AddItem(itemDefinition);
            SelectItem(itemDefinition);
            m_CurrentItemId = m_NewItemId;
            DrawGeneralDetail(itemDefinition);
        }

        protected override void AddItem(InventoryItemDefinition item)
        {
            EditorAPIHelper.AddItemDefinitionToInventoryCatalog(item);
            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            window.Repaint();
        }

        protected override void DrawDetail(InventoryItemDefinition inventoryItemDefinition, int index, int count)
        {
            DrawGeneralDetail(inventoryItemDefinition);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(inventoryItemDefinition, EditorAPIHelper.GetInventoryCatalogCategoriesList());

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(inventoryItemDefinition);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(inventoryItemDefinition, EditorAPIHelper.GetInventoryCatalogCategoriesList());
        }

        void DrawGeneralDetail(InventoryItemDefinition inventoryItemDefinition)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = inventoryItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (inventoryItemDefinition.displayName != displayName)
                {
                    inventoryItemDefinition.displayName = displayName;
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }

                ReferenceDefinitionPickerEditor.DrawReferenceDefinitionPicker(
                    inventoryItemDefinition,
                    new List<GameItemDefinition>(GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions()));
            }
        }

        protected override void DrawSidebarList()
        {
            EditorGUILayout.Space();

            bool categoryChanged;
            CategoryFilterEditor.DrawCategoryFilter(out categoryChanged);

            if (categoryChanged)
            {
                if (m_SelectedItem == null || !m_SelectedItem.HasCategoryDefinition(CategoryFilterEditor.GetCurrentFilteredCategory(EditorAPIHelper.GetInventoryCatalogCategoriesList())))
                {
                    SelectFilteredItem(0);
                }
            }

            base.DrawSidebarList();
        }

        protected override void DrawSidebarListItem(InventoryItemDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(InventoryItemDefinition item)
        {
            m_CategoryPicker.ResetCategorySearch();

            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(m_Items.Select(i => i.id)));
                m_CurrentItemId = item.id;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(InventoryItemDefinition item)
        {
            // If an inventory item is deleted, remove it from list and handle removing its asset as well
            if (item != null)
            {
                CollectionEditorTools.AssetDatabaseRemoveObject(item);
                EditorAPIHelper.RemoveItemDefinitionFromInventoryCatalog(item);
                EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            }
        }
    }
}
