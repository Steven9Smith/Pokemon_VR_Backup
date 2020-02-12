#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This consolidates all catalogs into one asset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameFoundationDatabase.asset", menuName = "Game Foundation/Database")]
    public class GameFoundationDatabase : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private GameItemCatalog m_GameItemCatalog;

        /// <summary>
        /// A reference to a game item catalog
        /// </summary>
        public GameItemCatalog gameItemCatalog
        {
            get { return m_GameItemCatalog; }
        }

        [SerializeField, HideInInspector]
        private InventoryCatalog m_InventoryCatalog;

        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public InventoryCatalog inventoryCatalog
        {
            get { return m_InventoryCatalog; }
        }

        [SerializeField, HideInInspector]
        private StatCatalog m_StatCatalog;

        /// <summary>
        /// A reference to a stat catalog
        /// </summary>
        public StatCatalog statCatalog
        {
            get { return m_StatCatalog; }
        }

#if UNITY_EDITOR
        public void VerifyDefaultCatalogs()
        {
            Tools.ThrowIfPlayMode("Cannot verify default catalogs while in play mode.");

            bool assetUpdate = false;

            if (m_GameItemCatalog == null)
            {
                m_GameItemCatalog = GameItemCatalog.Create();
                m_GameItemCatalog.name = "_Catalog_GameItems";
                AssetDatabase.AddObjectToAsset(m_GameItemCatalog, this);
                assetUpdate = true;
            }

            if (m_InventoryCatalog == null)
            {
                m_InventoryCatalog = InventoryCatalog.Create();
                m_InventoryCatalog.name = "_Catalog_Inventories";
                AssetDatabase.AddObjectToAsset(m_InventoryCatalog, this);
                m_InventoryCatalog.VerifyDefaultCollections();
                assetUpdate = true;
            }

            if (m_StatCatalog == null)
            {
                m_StatCatalog = StatCatalog.Create();
                m_StatCatalog.name = "_Catalog_Stats";
                AssetDatabase.AddObjectToAsset(m_StatCatalog, this);
                assetUpdate = true;
            }

            if (assetUpdate)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
#endif
    }
}
