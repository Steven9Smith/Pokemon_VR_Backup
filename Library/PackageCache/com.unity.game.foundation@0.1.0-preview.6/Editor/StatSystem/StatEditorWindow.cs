using System.Collections.Generic;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates Stat system-specific editor window.
    /// </summary>
    internal class StatEditorWindow : CollectionEditorWindowBase
    {
        private static List<ICollectionEditor> m_StatEditors = new List<ICollectionEditor>();
        protected override List<ICollectionEditor> m_Editors
        {
            get
            {
                return m_StatEditors;
            }
        }

        /// <summary>
        /// Opens the Stat window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<StatEditorWindow>(false, "Stat", true);
        }

        /// <summary>
        /// Adds the editors for the Stat system as tabs in the window.
        /// </summary>
        public override void CreateEditors()
        {
            m_StatEditors.Clear();

           m_StatEditors.Add(new StatDefinitionEditor("Stats", this));
        }
    }
}
