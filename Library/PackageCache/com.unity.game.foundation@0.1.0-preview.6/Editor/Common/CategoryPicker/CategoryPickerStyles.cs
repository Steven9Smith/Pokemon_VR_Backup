using UnityEngine;

namespace UnityEditor.GameFoundation
{
    internal class CategoryPickerStyles
    {

        private static GUIStyle s_SearchSuggestAreaStyle;
        public static GUIStyle searchSuggestAreaStyle
        {
            get
            {
                if (s_SearchSuggestAreaStyle == null)
                {
                    s_SearchSuggestAreaStyle = new GUIStyle(GUI.skin.textArea);
                }

                return s_SearchSuggestAreaStyle;
            }
        }

        private static GUIStyle s_CategoryListItemStyle;
        public static GUIStyle categoryListItemStyle
        {
            get
            {
                if (s_CategoryListItemStyle == null)
                {
                    s_CategoryListItemStyle = new GUIStyle(EditorStyles.helpBox);
                    s_CategoryListItemStyle.fontSize = 10;
                    s_CategoryListItemStyle.fontStyle = FontStyle.Bold;
                    s_CategoryListItemStyle.padding = new RectOffset(8, 7, 6, 6);
                    s_CategoryListItemStyle.wordWrap = false;
                    s_CategoryListItemStyle.clipping = TextClipping.Overflow;
                    s_CategoryListItemStyle.alignment = TextAnchor.MiddleLeft;
                }

                return s_CategoryListItemStyle;
            }
        }

        private static GUIStyle s_CategorySuggestItemStyle;
        public static GUIStyle categorySuggestItemStyle
        {
            get
            {
                if (s_CategorySuggestItemStyle == null)
                {
                    s_CategorySuggestItemStyle = new GUIStyle();
                    s_CategorySuggestItemStyle.padding = new RectOffset(5, 4, 3, 3);
                    // grab the default text color - it could be white or black depending on whether the editor skin is Personal or Professional
                    s_CategorySuggestItemStyle.normal.textColor = GUI.skin.label.normal.textColor;
                }

                return s_CategorySuggestItemStyle;
            }
        }

        private static GUIStyle s_CategorySuggestItemStyleSelected;
        public static GUIStyle categorySuggestItemStyleSelected
        {
            get
            {
                if (s_CategorySuggestItemStyleSelected == null)
                {
                    s_CategorySuggestItemStyleSelected = new GUIStyle(s_CategorySuggestItemStyle);
                    s_CategorySuggestItemStyleSelected.normal.background = (Texture2D)EditorGUIUtility.IconContent("selected").image;
                    s_CategorySuggestItemStyleSelected.normal.textColor = Color.white;
                }

                return s_CategorySuggestItemStyleSelected;
            }
        }

        private static int s_CategoryAddButtonWidth = 75;
        public static int categoryAddButtonWidth
        {
            get
            {
                return s_CategoryAddButtonWidth;
            }
        }

        private static int s_CategoryItemMargin = 6;
        public static int categoryItemMargin
        {
            get
            {
                return s_CategoryItemMargin;
            }
        }

        private static int s_CategoryRemoveButtonSpaceWidth = 15;
        public static int categoryRemoveButtonSpaceWidth
        {
            get
            {
                return s_CategoryRemoveButtonSpaceWidth;
            }
        }
    }
}
