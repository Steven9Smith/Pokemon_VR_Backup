namespace UnityEditor.GameFoundation
{
    internal static class MenuItems
    {
        /// <summary>
        /// Creates menu item for GameItem system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Game Item", false, 0)]
        public static void ShowGameItemsWindow()
        {
            GameItemEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for inventory system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Inventory", false, 0)]
        public static void ShowInventoriesWindow()
        {
            InventoryEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for stats system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Stat", false, 0)]
        public static void ShowStatWindow()
        {
            StatEditorWindow.ShowWindow();
        }
        
        [MenuItem("Window/Game Foundation/Tools/Delete Local Persistence Data", false, 2000)]
        public static void DeleteRuntimeData()
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete Game Foundation\'s runtime data?", "Yes", "No"))
            {
                UnityEngine.GameFoundation.Tools.DeleteRuntimeData();
            }
    }
    }
}
