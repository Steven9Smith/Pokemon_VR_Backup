using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Build
{
    static class StylesUtility
    {
        public static void AddStyleSheetAndVariant(this VisualElement ve, string styleSheetName)
        {
            ve.styleSheets.Add(LoadStyleSheetResource(styleSheetName));
            ve.styleSheets.Add(LoadStyleSheetResource($"{styleSheetName}_{(EditorGUIUtility.isProSkin ? "dark" : "light")}"));
        }

        static StyleSheet LoadStyleSheetResource(string name)
        {
            return Package.LoadResource<StyleSheet>("Editor/Unity.Build/GUI/uss", name + ".uss");
        }
    }
}
