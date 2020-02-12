using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    internal abstract class CollectionEditorBase<T> : ICollectionEditor where T : class
    {
        public string name { get; protected set; }
        public CollectionEditorWindowBase window { get; protected set; }

        protected T m_SelectedItem { get; set; }
        protected T m_PreviouslySelectedItem = null;
        protected abstract List<T> m_Items { get; }
        protected abstract List<T> m_FilteredItems { get; }

        protected Vector2 m_ScrollPosition;
        protected Vector2 m_ScrollPositionDetail;

        protected Rect m_SidebarItemOffset;

        private T m_ItemToRemove = null;

        protected bool m_IsCreating = false;
        public bool isCreating { get { return m_IsCreating; } }

        protected static string m_NewItemDisplayName = string.Empty;
        protected static string m_NewItemId = string.Empty;

        protected ReadableNameIdEditor m_ReadableNameIdEditor;

        public CollectionEditorBase(string name, CollectionEditorWindowBase window)
        {
            this.name = name;
            this.window = window;
        }

        // Update
        public virtual void Update()
        {
            ClearAndRemoveItems();

            Draw();
        }

        // Draw
        protected virtual void Draw()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            DrawSidebar();
            DrawContent();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        protected abstract void DrawDetail(T item, int index, int count);

        protected void DrawCreateForm()
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                DrawCreateInputFields();
                DrawWarningMessage();

                GUILayout.Space(6f);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Cancel", GUILayout.Width(120f)))
                    {
                        m_IsCreating = false;
                        SelectItem(m_PreviouslySelectedItem);
                        m_PreviouslySelectedItem = null;
                    }

                    GUILayout.Space(6f);

                    if ((string.IsNullOrEmpty(m_NewItemDisplayName)
                        || string.IsNullOrEmpty(m_NewItemId)) 
                        || m_ReadableNameIdEditor.HasRegisteredId(m_NewItemId)
                        || !CollectionEditorTools.IsValidId(m_NewItemId))
                    {
                        CollectionEditorTools.SetGUIEnabledAtEditorTime(false);
                    }
                    if (GUILayout.Button("Create", GUILayout.Width(120f)))
                    {
                        CreateNewItemFinalize();

                        m_IsCreating = false;
                    }
                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                }
            }
        }

        protected virtual void DrawCreateInputFields()
        {
            m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_NewItemId, ref m_NewItemDisplayName);
        }

        protected virtual void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox("Once the Create button is clicked Id cannot be changed.", MessageType.Warning);
        }

        // Enter - Exit
        public virtual void OnWillEnter()
        {
            m_IsCreating = false;

            SelectItem(null);
        }

        public virtual void OnWillExit()
        {
            m_IsCreating = false;

            SelectItem(null);
        }


        // Side Bar
        protected virtual void DrawSidebar()
        {
            BeginSidebar();
            DrawSidebarContent();
            EndSidebar();
        }

        protected virtual void BeginSidebar()
        {
            EditorGUILayout.BeginVertical(GameFoundationEditorStyles.sideBarStyle, GUILayout.Width(GetSideBarWidth()));
        }

        protected virtual float GetSideBarWidth()
        {
            return 270f;
        }

        protected virtual void DrawSidebarContent()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawSidebarList();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+", GameFoundationEditorStyles.createButtonStyle))
            {
                if (!m_IsCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    m_PreviouslySelectedItem = m_SelectedItem;
                    m_IsCreating = true;
                    m_NewItemDisplayName = string.Empty;
                    m_NewItemId = string.Empty;
                    SelectItem(null);
                    CreateNewItem();
                }
            }
        }

        protected virtual void DrawSidebarList()
        {
            var filteredItems = m_FilteredItems;
            for (int i = 0; i < filteredItems.Count; ++i)
            {
                DrawSidebarListItem(filteredItems[i], i);
            }
        }

        protected virtual void EndSidebar()
        {
            EditorGUILayout.EndHorizontal();
        }

        protected abstract void DrawSidebarListItem(T item, int index);


        // Side Bar Items
        protected virtual void BeginSidebarItem(T item, int index, Vector2 size)
        {
            BeginSidebarItem(item, index, size, Vector2.zero);
        }

        protected virtual void BeginSidebarItem(T item, int index, Vector2 backgroundSize, Vector2 contentMargin)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, backgroundSize.y);
            rect.width = backgroundSize.x;

            GUI.backgroundColor =
                item.Equals(m_SelectedItem) ? new Color(0.1f, 0.1f, 0.1f, .2f) : new Color(0, 0, 0, 0.0f);

            CollectionEditorTools.SetGUIEnabledAtRunTime(true);
            if (GUI.Button(rect, string.Empty))
            {
                if (!m_IsCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    // if you click an item in the list, then cancel any creation in progress
                    m_IsCreating = false;

                    SelectItem(item);
                }
            }
            CollectionEditorTools.SetGUIEnabledAtRunTime(false);

            GUI.backgroundColor = Color.white;

            m_SidebarItemOffset = rect;
            m_SidebarItemOffset.x += contentMargin.x;
            m_SidebarItemOffset.y += contentMargin.y;

            GUI.color = m_SelectedItem == item ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(backgroundSize.y));
        }

        protected virtual void EndSidebarItem(T item, int index)
        {
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawSidebarItemLabel(string text, int width, GUIStyle style, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = (height == -1) ? m_SidebarItemOffset.height : height;

            if (style == null)
                EditorGUI.LabelField(m_SidebarItemOffset, text);
            else
                EditorGUI.LabelField(m_SidebarItemOffset, text, style);

            m_SidebarItemOffset.x += width;
        }

        protected virtual bool DrawSidebarItemButton(string text, GUIStyle style, int width, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = (height == -1) ? m_SidebarItemOffset.height : height;

            bool clicked = GUI.Button(m_SidebarItemOffset, text, style);

            m_SidebarItemOffset.x += width;

            return clicked;
        }

        protected virtual void DrawSidebarItemRemoveButton(T item)
        {
            if (DrawSidebarItemButton("X", GameFoundationEditorStyles.deleteButtonStyle, 18, 18))
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + name + "?", "Yes", "No"))
                {
                    RemoveItem(item);
                }
            }
        }

        // Content
        protected virtual void DrawContent()
        {
            BeginContent();
            DrawContentDetail();
            EndContent();
        }

        protected virtual void BeginContent()
        {
            m_ScrollPositionDetail = EditorGUILayout.BeginScrollView(m_ScrollPositionDetail, GUILayout.MaxWidth(GetContentDetailMaxWidth()));
        }

        protected virtual float GetContentDetailMaxWidth()
        {
            return 630f;
        }

        protected virtual void EndContent()
        {
            EditorGUILayout.EndScrollView();
        }

        public void ValidateSelection()
        {
            // it's possible that the selected item was deleted or a new database was loaded
            if (m_Items != null && !m_Items.Contains(m_SelectedItem))
            {
                SelectItem(null);
            }
        }

        protected virtual void DrawContentDetail()
        {
            if (m_SelectedItem != null)
            {
                m_IsCreating = false;

                DrawDetail(m_SelectedItem, m_Items.FindIndex(x => x.Equals(m_SelectedItem)), m_Items.Count);
            }
            else if (m_IsCreating)
            {
                DrawCreateForm();
            }
            else
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("No object selected.");
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        // Actions
        protected abstract void CreateNewItem();
        protected abstract void CreateNewItemFinalize();
        protected abstract void AddItem(T item);
        protected abstract void OnRemoveItem(T item);

        protected virtual void RemoveItem(T item)
        {
            m_ItemToRemove = item;
        }

        protected void ClearAndRemoveItems()
        {
            if (m_Items == null)
            {
                return;
            }

            // Remove Item
            if (m_ItemToRemove != null)
            {
                OnRemoveItem(m_ItemToRemove);
                m_ItemToRemove = null;
                SelectItem(null);
                window.Repaint();
            }
        }

        protected virtual void SelectItem(T item)
        {
            m_SelectedItem = item;

            ClearFocus();
        }

        protected void ClearFocus()
        {
            GUI.FocusControl(null);
        }

        protected virtual void SelectFilteredItem(int listIndex)
        {
            if (m_FilteredItems != null && m_FilteredItems.Count > listIndex && listIndex >= 0)
            {
                SelectItem(m_FilteredItems[listIndex]);
            }
            else
            {
                SelectItem(null);
            }
        }
    }
}
