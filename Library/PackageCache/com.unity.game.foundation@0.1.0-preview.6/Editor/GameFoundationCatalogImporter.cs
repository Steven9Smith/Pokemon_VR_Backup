using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This class uses the AssetPostProcessor to verify the GameFoundation's default catalogs.
    /// This is a catch all solution that will verify the contents anytime this file is created or modified.
    /// </summary>
    class GameFoundationCatalogImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            VerifyPaths(importedAssets);
            VerifyPaths(movedAssets);
            VerifyPaths(movedFromAssetPaths);
        }

        private static void VerifyPaths(string[] assetPaths)
        {
            foreach (string importedAsset in assetPaths)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<GameFoundationDatabase>(importedAsset);
                if (catalog != null)
                {
                    catalog.VerifyDefaultCatalogs();
                }
            }
        }
    }
}