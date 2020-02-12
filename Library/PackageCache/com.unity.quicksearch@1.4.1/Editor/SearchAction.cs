using UnityEngine;

namespace Unity.QuickSearch
{
    public class SearchAction
    {
        public const string kContextualMenuAction = "context";
        public SearchAction(string type, GUIContent content)
        {
            providerId = type;
            this.content = content;
            isEnabled = (item, context) => true;
        }

        public SearchAction(string type, string name, Texture2D icon = null, string tooltip = null)
            : this(type, new GUIContent(name, icon, tooltip ?? name))
        {
        }

        public string Id => content.text;
        public string DisplayName => content.tooltip;
        public bool closeWindowAfterExecution = true;

        // Unique (for a given provider) id of the action
        public string providerId;
        public GUIContent content;
        // Called when an item is executed with this action
        public ActionHandler handler;
        // Called before displaying the menu to see if an action is available for a given item.
        public EnabledHandler isEnabled;
    }
}