using System.Collections.Generic;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates GameItem system-specific editor window.
    /// </summary>
    internal class GameItemEditorWindow : CollectionEditorWindowBase
    {
        
        private GameItemCatalog m_SelectedAssetToLoad;
        private GameItemCatalog m_NewSelectedAssetToLoad;

        private static List<ICollectionEditor> m_GameItemEditors = new List<ICollectionEditor>();
        protected override List<ICollectionEditor> m_Editors
        {
            get { return m_GameItemEditors; }
        }

        /// <summary>
        /// Opens the GameItem window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<GameItemEditorWindow>(false, "Game Item", true);
        }

        /// <summary>
        /// Adds the editors for the GameItem system as tabs in the window.
        /// </summary>
        public override void CreateEditors()
        {
            m_GameItemEditors.Clear();

            m_GameItemEditors.Add(new GameItemDefinitionEditor("GameItem Items", this));
        }
    }
}
