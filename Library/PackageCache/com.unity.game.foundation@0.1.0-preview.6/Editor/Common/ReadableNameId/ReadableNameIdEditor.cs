using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// UI Module for displayName and Id fields.
    /// Contains logic for converting displayName to a Id
    /// suggestion when Id field is empty.
    /// </summary>
    internal class ReadableNameIdEditor
    {
        private bool m_AutomaticIdGenerationMode = true;
        private bool m_IdEditingAllowedMode = true;
        private HashSet<string> m_OldIds = new HashSet<string>();

        public ReadableNameIdEditor(bool createNewMode, HashSet<string> oldIds)
        {
            m_AutomaticIdGenerationMode = createNewMode;
            m_IdEditingAllowedMode = createNewMode;

            m_OldIds = oldIds;
        }

        /// <summary>
        /// Draws UI input fields for displayName and Id.
        /// Will create a suggested Id based on input displayName based
        /// on the following conditions: displayName has a value, Id has
        /// not been edited manually or is blank, displayName field loses focus by
        /// tab or mouse click event, and item is being created new (vs editing existing item).
        /// ref parameters may change what has been passed in.
        /// </summary>
        /// <param name="itemId">Text to display for Id text field.</param>
        /// <param name="displayName">Text to display for display name text field.</param>
        public void DrawReadableNameIdFields(ref string itemId, ref string displayName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUI.SetNextControlName("displayName");
                displayName = EditorGUILayout.TextField("Display Name", displayName);

                if (m_IdEditingAllowedMode)
                {
                    ConvertIdIfNecessary(ref itemId, ref displayName);

                    GUI.changed = false;
                    GUI.SetNextControlName("id");
                    itemId = EditorGUILayout.TextField("Id", itemId);
                    if (GUI.changed)
                    {
                        m_AutomaticIdGenerationMode = false;
                    }

                    if (HasRegisteredId(itemId))
                    {
                        EditorGUILayout.HelpBox("The current Id will conflict with existing Ids.", MessageType.Error);
                    }
                    else if (!string.IsNullOrWhiteSpace(itemId) && !CollectionEditorTools.IsValidId(itemId))
                    {
                        EditorGUILayout.HelpBox("The current Id is not valid. Ensure it is alphanumeric with optional - and _ characters", MessageType.Error);
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Id", GUILayout.Width(145));
                        EditorGUILayout.SelectableLabel(itemId, GUILayout.Height(15), GUILayout.ExpandWidth(true));
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the proposed Id will conflict with those registered in the system that this ReadableNameIdEditor object exists in.
        /// </summary>
        /// <param name="itemId">Id to check</param>
        /// <returns>Whether this Id is prolematic</returns>
        public bool HasRegisteredId(string itemId)
        {
            return m_OldIds.Contains(itemId);
        }
        
        private void ConvertIdIfNecessary(ref string itemId, ref string displayName)
        {
            Event e = Event.current;
            bool desiredEvent = e.Equals(Event.KeyboardEvent("tab")) || e.type.Equals(EventType.MouseDown);
            bool desiredControlFocus = GUI.GetNameOfFocusedControl() == "displayName" || GUI.GetNameOfFocusedControl() == "id";

            if (desiredEvent && desiredControlFocus && !String.IsNullOrEmpty(displayName) && (m_AutomaticIdGenerationMode || String.IsNullOrEmpty(itemId)))
            {
                if (String.IsNullOrEmpty(itemId))
                {
                    m_AutomaticIdGenerationMode = true;
                }

                itemId = CollectionEditorTools.CraftUniqueId(displayName, m_OldIds);
            }
        }
    }
}
