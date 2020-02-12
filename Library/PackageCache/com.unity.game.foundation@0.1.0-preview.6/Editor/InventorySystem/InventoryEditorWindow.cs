using System.Collections.Generic;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates Inventory system-specific editor window.
    /// </summary>
    internal class InventoryEditorWindow : CollectionEditorWindowBase
    {

        private InventoryCatalog m_SelectedAssetToLoad;
        private InventoryCatalog m_NewSelectedAssetToLoad;

        private static List<ICollectionEditor> m_InventoryEditors = new List<ICollectionEditor>();

        protected override List<ICollectionEditor> m_Editors
        {
            get { return m_InventoryEditors; }
        }

        /// <summary>
        /// Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<InventoryEditorWindow>(false, "Inventory", true);
        }

        /// <summary>
        /// Adds the editors for the inventory system as tabs in the window.
        /// </summary>
        public override void CreateEditors()
        {
            m_InventoryEditors.Clear();

            m_InventoryEditors.Add(new InventoryItemDefinitionEditor("Inventory Items", this));
            m_InventoryEditors.Add(new InventoryDefinitionEditor("Inventories", this));
            m_InventoryEditors.Add(new CategoryDefinitionEditor("Categories", this));
        }
    }
}
