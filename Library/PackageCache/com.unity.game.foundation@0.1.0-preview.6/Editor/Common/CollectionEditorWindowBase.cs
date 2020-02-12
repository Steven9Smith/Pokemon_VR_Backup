using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Abstract class which sets up an editor window with tabs toolbar.
    /// </summary>
    internal abstract class CollectionEditorWindowBase : EditorWindow
    {
        protected int m_OldToolbarIndex { get; set; }
        protected int m_ToolbarIndex { get; set; }

        protected abstract List<ICollectionEditor> m_Editors { get; }

        protected string[] editorNames
        {
            get
            {
                var items = new string[m_Editors.Count];
                for (int i = 0; i < m_Editors.Count; i++)
                {
                    items[i] = m_Editors[i].name;
                }

                return items;
            }
        }

        protected virtual void OnEnable()
        {
            minSize = new Vector2(850f, 400f);

            m_ToolbarIndex = 0;
            m_OldToolbarIndex = -1;

            CreateEditors();

            if (Resources.Load<GameFoundationSettings>("GameFoundationSettings") == null)
            {
                Debug.LogWarning("No Game Foundation settings file has been found. Game Foundation code will automatically create one. Settings file is critical to Game Foundation, if you wish to remove it you will need to remove the entire Game Foundation package.");
            }
        }

        protected virtual void OnFocus()
        {
            foreach (ICollectionEditor collectionEditor in m_Editors)
            {
                collectionEditor.ValidateSelection();
            }
        }

        /// <summary>
        /// Abstract method where editors for the implementing system's tabs should be added to the window.
        /// </summary>
        public abstract void CreateEditors();

        protected virtual void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            m_ToolbarIndex = GUILayout.Toolbar(m_ToolbarIndex, editorNames, GameFoundationEditorStyles.topToolbarStyle);

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            DrawCollectionEditor();
        }

        protected void DrawCollectionEditor()
        {
            DrawDefaultState();
        }

        /// <summary>
        /// Draw the entire UI for this type of collection editor window.
        /// </summary>
        public virtual void DrawDefaultState()
        {
            if (m_Editors.Count > 1)
            {
                DrawToolbar();
            }

            EditorGUILayout.Space();

            if (m_ToolbarIndex < 0 || m_ToolbarIndex >= m_Editors.Count || m_Editors.Count == 0)
            {
                m_ToolbarIndex = 0;
                CreateEditors();
            }

            // Enter and Exit
            if (m_ToolbarIndex != m_OldToolbarIndex)
            {
                // if trying to switch tabs while in the middle of creating a new item, show a confirmation dialog
                if (m_OldToolbarIndex >= 0
                    && m_Editors[m_OldToolbarIndex].isCreating
                    && !CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    // clicked "Stay" - prevent switching tabs - change the index back
                    m_ToolbarIndex = m_OldToolbarIndex;
                    return;
                }

                if (m_OldToolbarIndex >= 0 && m_OldToolbarIndex < m_Editors.Count)
                {
                    m_Editors[m_OldToolbarIndex].OnWillExit();
                }

                m_Editors[m_ToolbarIndex].OnWillEnter();

                m_OldToolbarIndex = m_ToolbarIndex;
            }

            // Update
            m_Editors[m_ToolbarIndex].Update();
        }
    }
}
