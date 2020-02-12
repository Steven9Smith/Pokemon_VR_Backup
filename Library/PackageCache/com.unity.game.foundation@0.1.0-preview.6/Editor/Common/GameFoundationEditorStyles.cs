using UnityEngine;

namespace UnityEditor.GameFoundation
{
    internal static class GameFoundationEditorStyles
    {
        public const string noValueLabelColor = "#808080";

        private static GUIStyle s_TopToolbarStyle;
        public static GUIStyle topToolbarStyle
        {
            get
            {
                if (s_TopToolbarStyle == null)
                {
                    s_TopToolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
                    s_TopToolbarStyle.fixedHeight = 40;                    
                }

                return s_TopToolbarStyle;
            }
        }
        
        private static GUIStyle s_BoxStyle;
        public static GUIStyle boxStyle
        {
            get
            {
                if (s_BoxStyle == null)
                {
                    s_BoxStyle = new GUIStyle("HelpBox");
                    s_BoxStyle.padding = new RectOffset(10, 10, 10, 10);
                }

                return s_BoxStyle;
            }
        }
        
        private static GUIStyle s_SideBarStyle;
        public static GUIStyle sideBarStyle
        {
            get
            {
                if (s_SideBarStyle == null)
                {
                    s_SideBarStyle = new GUIStyle("HelpBox");
                    s_SideBarStyle.padding = new RectOffset(10, 10, 10, 10);
                    s_SideBarStyle.margin = new RectOffset(10, 5, 5, 5);
                }

                return s_SideBarStyle;
            }
        }

        private static GUIStyle m_RichTextLabelStyle;
        internal static GUIStyle richTextLabelStyle
        {
            get
            { 
                if (m_RichTextLabelStyle == null)
                {
                    m_RichTextLabelStyle = new GUIStyle(EditorStyles.label);
                    m_RichTextLabelStyle.richText = true;
                }
                return m_RichTextLabelStyle;
            }
        }

        private static GUIStyle s_RichTextArea;
        public static GUIStyle richTextArea
        {
            get
            {
                if (s_RichTextArea == null)
                {
                    s_RichTextArea = new GUIStyle(EditorStyles.textArea);
                    s_RichTextArea.richText = true;
                    s_RichTextArea.wordWrap = true;
                    s_RichTextArea.fixedHeight = 40.0f;
                    s_RichTextArea.stretchHeight = true;
                }

                return s_RichTextArea;
            }
        }
        
        private static GUIStyle s_TitleStyle;
        public static GUIStyle titleStyle
        {
            get
            {
                if (s_TitleStyle == null)
                {
                    s_TitleStyle = new GUIStyle("IN TitleText");
                    s_TitleStyle.alignment = TextAnchor.MiddleLeft;
                    s_TitleStyle.padding.left += 5;
                    s_TitleStyle.margin.top += 10;
                }

                return s_TitleStyle;
            }
        }
        
        private static GUIStyle s_ItalicTextStyle;
        public static GUIStyle italicTextStyle
        {
            get
            {
                if (s_ItalicTextStyle == null)
                {
                    s_ItalicTextStyle = new GUIStyle(EditorStyles.label);
                    s_ItalicTextStyle.fontStyle = FontStyle.Italic;
                }

                return s_ItalicTextStyle;
            }
        }
        
        private static GUIStyle s_BoldTextStyle;
        public static GUIStyle boldTextStyle
        {
            get
            {
                if (s_BoldTextStyle == null)
                {
                    s_BoldTextStyle = new GUIStyle(EditorStyles.label);
                    s_BoldTextStyle.fontStyle = FontStyle.Bold;
                }

                return s_BoldTextStyle;
            }
        }

        private static GUIStyle s_CenterAlignedTextStyle;
        public static GUIStyle centerAlignedText
        {
            get
            {
                if (s_CenterAlignedTextStyle == null)
                {
                    s_CenterAlignedTextStyle = new GUIStyle(EditorStyles.label);
                    s_CenterAlignedTextStyle.alignment = TextAnchor.UpperCenter;
                }

                return s_CenterAlignedTextStyle;
            }
        }

        private static GUIStyle s_CenteredGrayLabel;
        public static GUIStyle centeredGrayLabel
        {
            get
            {
                if (s_CenterAlignedTextStyle == null)
                {
                    s_CenterAlignedTextStyle = new GUIStyle(EditorStyles.label);
                    s_CenterAlignedTextStyle.alignment = TextAnchor.UpperCenter;
                    s_CenterAlignedTextStyle.normal.textColor = Color.gray;
                }

                return s_CenterAlignedTextStyle;
            }
        }
        
        private static GUIStyle s_RightAlignedTextStyle;
        public static GUIStyle rightAlignedTextStyle
        {
            get
            {
                if (s_RightAlignedTextStyle == null)
                {
                    s_RightAlignedTextStyle = new GUIStyle(EditorStyles.label);
                    s_RightAlignedTextStyle.alignment = TextAnchor.MiddleRight;
                }

                return s_RightAlignedTextStyle;
            }
        }
        
        private static GUIStyle s_CreateButtonStyle;
        public static GUIStyle createButtonStyle
        {
            get
            {
                if (s_CreateButtonStyle == null)
                {
                    s_CreateButtonStyle = new GUIStyle(EditorStyles.miniButton);
                    s_CreateButtonStyle.fixedHeight = 20f;
                    s_CreateButtonStyle.fontStyle = FontStyle.Bold;
                    s_CreateButtonStyle.fontSize = 12;
                }

                return s_CreateButtonStyle;
            }
        }
        
        private static GUIStyle s_DeleteButtonStyle;
        public static GUIStyle deleteButtonStyle
        {
            get
            {
                if (s_DeleteButtonStyle == null)
                {
                    s_DeleteButtonStyle = new GUIStyle("OL Minus");
                }

                return s_DeleteButtonStyle;
            }
        }
        
        private static GUIStyle s_TableViewToolbarStyle;
        public static GUIStyle tableViewToolbarStyle
        {
            get
            {
                if (s_TableViewToolbarStyle == null)
                {
                    s_TableViewToolbarStyle = new GUIStyle(EditorStyles.toolbarButton);              
                }

                return s_TableViewToolbarStyle;
            }
        }
        
        private static GUIStyle s_TableViewToolbarTextStyle;
        public static GUIStyle tableViewToolbarTextStyle
        {
            get
            {
                if (s_TableViewToolbarTextStyle == null)
                {
                    s_TableViewToolbarTextStyle = new GUIStyle(EditorStyles.miniLabel);              
                }

                return s_TableViewToolbarTextStyle;
            }
        }
        
        private static GUIStyle s_TableViewButtonStyle;
        public static GUIStyle tableViewButtonStyle
        {
            get
            {
                if (s_TableViewButtonStyle == null)
                {
                    s_TableViewButtonStyle = new GUIStyle("toolbarbutton");
                }

                return s_TableViewButtonStyle;
            }
        }
    }
}
