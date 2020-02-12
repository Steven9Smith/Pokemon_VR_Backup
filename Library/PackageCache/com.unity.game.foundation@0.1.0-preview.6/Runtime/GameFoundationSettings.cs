#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation settings for all of Game Foundation implemenation and serialization.
    /// </summary>
    public class GameFoundationSettings : ScriptableObject
    {
        /// <summary>
        /// The directory name where Unity project assets will be created/stored.
        /// </summary>
        public static readonly string kAssetsFolder = "GameFoundation";

        private static GameFoundationSettings s_Instance;
        internal static GameFoundationSettings singleton
        {
            get
            {
                bool assetUpdate = false;

                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");

#if UNITY_EDITOR
                    if (s_Instance == null && !Application.isPlaying)
                    {
                        s_Instance = ScriptableObject.CreateInstance<GameFoundationSettings>();

                        if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}", kAssetsFolder)))
                        {
                            AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                        }

                        if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}/Resources", kAssetsFolder)))
                        {
                            AssetDatabase.CreateFolder(string.Format("Assets/{0}", kAssetsFolder), "Resources");
                        }

                        AssetDatabase.CreateAsset(s_Instance, string.Format("Assets/{0}/Resources/GameFoundationSettings.asset", kAssetsFolder));
                        assetUpdate = true;

                        s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");
                    }
#endif

                    if (s_Instance == null)
                    {
                        throw new System.InvalidOperationException("Unable to find or create a GameFoundationSettings resource!");
                    }
                }

#if UNITY_EDITOR
                if (s_Instance.m_Database == null)
                {
                    Tools.ThrowIfPlayMode("Game Foundation database reference cannot be null while in play mode. "
                        + "Open one of the Game Foundation windows in the Unity Editor while not in Play Mode to have a database asset created for you automatically.");

                    string databaseAssetPath = $"Assets/{kAssetsFolder}/GameFoundationDatabase.asset";

                    // try to load a database asset by hardcoded path
                    s_Instance.m_Database = AssetDatabase.LoadAssetAtPath<GameFoundationDatabase>(databaseAssetPath);

                    // if that doesn't work, then create one
                    if (s_Instance.m_Database == null)
                    {
                        s_Instance.m_Database = ScriptableObject.CreateInstance<GameFoundationDatabase>();

                        if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}", kAssetsFolder)))
                        {
                            AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                        }

                        AssetDatabase.CreateAsset(s_Instance.m_Database, databaseAssetPath);
                        EditorUtility.SetDirty(s_Instance);
                        assetUpdate = true;
                    }
                }

                if (assetUpdate)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
#else
                if (s_Instance.m_Database == null)
                {
                    throw new System.Exception("Game Foundation database reference cannot be null."
                        + "Open one of the Game Foundation windows in the Unity Editor while not in Play Mode to have a database asset created for you automatically.");
                }
#endif

                return s_Instance;
            }
        }

        [SerializeField]
        private GameFoundationDatabase m_Database;

        public static GameFoundationDatabase database
        {
            get { return singleton.m_Database; }
            set { singleton.m_Database = value; }
        }

        [SerializeField]
        private bool m_EnablePlayModeAnalytics = true;

        /// <summary>
        /// Indicates whether analytics events should be fired while in Play Mode.
        /// </summary>
        /// <returns>True if analytics events should be fired while in Play Mode.</returns>
        public static bool enablePlayModeAnalytics
        {
            get { return singleton.m_EnablePlayModeAnalytics; }
            set {
                singleton.m_EnablePlayModeAnalytics = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(s_Instance);
#endif
            }
        }

        [SerializeField]
        private bool m_EnableEditorModeAnalytics = false;

        /// <summary>
        /// Indicates whether analytic events should be fired while in Editor Mode.
        /// </summary>
        /// <returns>True if analytic events should be fired while in Editor Mode.</returns>
        public static bool enableEditorModeAnalytics
        {
            get { return singleton.m_EnableEditorModeAnalytics; }
            set {
                singleton.m_EnableEditorModeAnalytics = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(s_Instance);
#endif
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Set GameFoundationSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings", false, 2000)]
        public static void SelectGameFoundationSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(singleton, null);
        }
#endif
    }
}
