using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryDefinitionEditor : CollectionEditorBase<InventoryDefinition>
    {
        private string m_InventoryId = null;
        private DefaultItem[] m_DefaultItems = null;
        private DefaultItem m_DefaultItemToMoveDown = null;
        private DefaultItem m_DefaultItemToMoveUp = null;
        private DefaultItem m_DefaultItemToRemove;
        private DefaultCollectionDefinition m_DefaultInventoryDefinition;
        private bool m_CreateDefaultInventory;
        private GUIContent m_AutoCreateInventoryLabel;

        protected override List<InventoryDefinition> m_Items
        {
            get
            {
                return EditorAPIHelper.GetInventoryCatalogCollectionDefinitionsList();
            }
        }

        protected override List<InventoryDefinition> m_FilteredItems
        {
            get { return m_Items; }
        }

        public InventoryDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
            m_AutoCreateInventoryLabel = new GUIContent("Auto Create Instance", "When checked, this InventoryDefinition is added to a list of DefaultCollectionDefinitions so that an instance of it will automatically be created at runtime initialization.");
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                return;
            }

            SelectFilteredItem(0); // Select the first Item
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(m_Items.Select(i => i.id)));
        }

        protected override void CreateNewItemFinalize()
        {
            InventoryDefinition inventory = EditorAPIHelper.CreateInventoryDefinition(m_NewItemId, m_NewItemDisplayName);

            if (inventory != null)
            {
                AddItem(inventory);
                CollectionEditorTools.AssetDatabaseAddObject(inventory, GameFoundationSettings.database.inventoryCatalog);
                SelectItem(inventory);
                m_InventoryId = m_NewItemId;
                DrawGeneralDetail(inventory, m_Items.FindIndex(x => x.Equals(m_SelectedItem)));
            }
            else
            {
                Debug.LogError("Sorry, there was an error creating new inventory. Please try again.");
            }
        }

        protected override void AddItem(InventoryDefinition inventoryDefinition)
        {
            EditorAPIHelper.AddInventoryDefinitionToInventoryCatalog(inventoryDefinition);
            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            window.Repaint();
        }

        protected override void DrawDetail(InventoryDefinition inventory, int index, int count)
        {
            DrawGeneralDetail(inventory, index);

            EditorGUILayout.Space();

            DrawInventoryDetail(inventory, index, count);
        }

        private void DrawGeneralDetail(InventoryDefinition inventoryDefinition, int index)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = inventoryDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_InventoryId, ref displayName);

                if (inventoryDefinition.displayName != displayName)
                {
                    inventoryDefinition.displayName = displayName;
                    EditorUtility.SetDirty(inventoryDefinition);
                }

                if (IsIdReserved(inventoryDefinition.id))
                {
                    GUI.enabled = false;
                }

                bool newAutoCreateInventorySelection = EditorGUILayout.Toggle(m_AutoCreateInventoryLabel, m_CreateDefaultInventory);
                if (newAutoCreateInventorySelection != m_CreateDefaultInventory)
                {
                    if (newAutoCreateInventorySelection)
                    {
                        m_DefaultInventoryDefinition = new DefaultCollectionDefinition(inventoryDefinition.id, inventoryDefinition.displayName, inventoryDefinition.hash);
                        GameFoundationSettings.database.inventoryCatalog.AddDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                    }
                    else
                    {
                        GameFoundationSettings.database.inventoryCatalog.RemoveDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                        m_DefaultInventoryDefinition = null;
                    }
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                    m_CreateDefaultInventory = newAutoCreateInventorySelection;
                }

                if (IsIdReserved(inventoryDefinition.id))
                {
                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                }
            }
        }

        private void DrawInventoryDetail(InventoryDefinition inventoryDefinition, int index, int count)
        {
            m_DefaultItems = inventoryDefinition.GetDefaultItems();

            DrawItemsInInventory(inventoryDefinition);

            EditorGUILayout.Space();

            DrawItemsNotInInventory(inventoryDefinition);
        }

        private void DrawItemsInInventory(InventoryDefinition inventoryDefinition)
        {
            m_DefaultItemToMoveUp = null;
            m_DefaultItemToMoveDown = null;
            m_DefaultItemToRemove = null;

            var inventoryCatalogAllItemDefinitions = GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions();

            EditorGUILayout.LabelField("Default Items", GameFoundationEditorStyles.titleStyle);

            EditorGUILayout.BeginVertical(GameFoundationEditorStyles.boxStyle);

            EditorGUILayout.LabelField("When this inventory is instantiated, these items will be created and added to it automatically.");
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal(GameFoundationEditorStyles.tableViewToolbarStyle);
            EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Quantity", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(80));
            GUILayout.Space(64);
            GUILayout.EndHorizontal();

            if (m_DefaultItems != null && m_DefaultItems.Count() > 0)
            {
                for (int i = 0; i < m_DefaultItems.Count(); i++)
                {
                    DefaultItem defaultInventoryItem = m_DefaultItems[i];
                    var inventoryItemDefinition = inventoryCatalogAllItemDefinitions.FirstOrDefault(item => item.hash == defaultInventoryItem.definitionHash);
                    if (inventoryItemDefinition != null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.LabelField(inventoryItemDefinition.displayName, GUILayout.Width(150));

                            GUILayout.FlexibleSpace();

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(inventoryItemDefinition != null);
                            int quantity = defaultInventoryItem.quantity;
                            quantity = EditorGUILayout.IntField(quantity, GUILayout.Width(80));

                            if (quantity != defaultInventoryItem.quantity)
                            {
                                inventoryDefinition.SetDefaultItemQuantity(defaultInventoryItem, quantity);
                                EditorUtility.SetDirty(inventoryDefinition);
                            }

                            GUILayout.Space(5);

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_DefaultItems.Count() - 1);
                            if (GUILayout.Button("\u25BC", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToMoveDown = defaultInventoryItem;
                                m_DefaultItemToMoveUp = m_DefaultItems[i + 1];
                            }
                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i > 0);
                            if (GUILayout.Button("\u25B2", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToMoveUp = defaultInventoryItem;
                                m_DefaultItemToMoveDown = m_DefaultItems[i - 1];
                            }
                            CollectionEditorTools.SetGUIEnabledAtEditorTime(true);

                            GUILayout.Space(5);

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToRemove = defaultInventoryItem;
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("no default items");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();

            if (m_DefaultItemToMoveUp != null && m_DefaultItemToMoveDown != null)
            {
                SwapInventoryItems(inventoryDefinition, m_DefaultItemToMoveUp, m_DefaultItemToMoveDown);
            }

            if (m_DefaultItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog ("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    inventoryDefinition.RemoveDefaultItem(m_DefaultItemToRemove);
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }
            }

            if (m_DefaultItemToMoveUp != null || m_DefaultItemToMoveDown != null || m_DefaultItemToRemove != null)
            {
                ClearFocus();
            }
        }

        private void DrawItemsNotInInventory(InventoryDefinition inventoryDefinition)
        {
            var inventoryCatalogAllItemDefinitions = GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions();

            EditorGUILayout.LabelField("Other Available Items", GameFoundationEditorStyles.titleStyle);

            EditorGUILayout.BeginVertical(GameFoundationEditorStyles.boxStyle);
            GUILayout.BeginHorizontal(GameFoundationEditorStyles.tableViewToolbarStyle);
            EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            int validItemCount = 0;

            foreach (InventoryItemDefinition inventoryItemDefinition in inventoryCatalogAllItemDefinitions)
            {
                // wallets can only have currencies as auto-add items
                if (inventoryDefinition.id == EditorAPIHelper.k_WalletInventoryDefinitionId &&
                    inventoryItemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    continue;
                }

                if (m_DefaultItems.Count() > 0 && m_DefaultItems.Any(defaultItem => defaultItem.definitionHash == inventoryItemDefinition.hash))
                {
                    continue;
                }

                validItemCount++;

                GUILayout.BeginHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.LabelField(inventoryItemDefinition.displayName);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add To Default Items", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(150)))
                {
                    inventoryDefinition.AddDefaultItem(inventoryItemDefinition);
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }

                GUILayout.EndHorizontal();
            }

            if (validItemCount <= 0)
            {
                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("no items available");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();
        }

        private void SwapInventoryItems(InventoryDefinition inventoryDefinition, DefaultItem defaultItem1, DefaultItem defaultItem2)
        {
            inventoryDefinition.SwapDefaultItemsListOrder(defaultItem1, defaultItem2);
            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
        }

        protected override void DrawSidebarListItem(InventoryDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            if (!IsIdReserved(item.id))
            {
                DrawSidebarItemRemoveButton(item);
            }

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(InventoryDefinition inventoryDefinition)
        {
            if (inventoryDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(m_Items.Select(i => i.id)));
                m_InventoryId = inventoryDefinition.id;
                m_DefaultInventoryDefinition = GameFoundationSettings.database.inventoryCatalog.GetDefaultCollectionDefinition(inventoryDefinition.id);
                m_CreateDefaultInventory = IsIdReserved(m_InventoryId) || m_DefaultInventoryDefinition != null;
            }

            base.SelectItem(inventoryDefinition);
        }

        protected override void OnRemoveItem(InventoryDefinition item)
        {
            // If an inventory item is deleted, handling removing its asset as well
            if (item != null)
            {
                CollectionEditorTools.AssetDatabaseRemoveObject(item);
                EditorAPIHelper.RemoveInventoryDefinitionFromInventoryCatalog(item);
                EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            }
        }

        protected static bool IsIdReserved(string id)
        {
            return id == EditorAPIHelper.k_MainInventoryDefinitionId || id == EditorAPIHelper.k_WalletInventoryDefinitionId;
        }
    }
}
