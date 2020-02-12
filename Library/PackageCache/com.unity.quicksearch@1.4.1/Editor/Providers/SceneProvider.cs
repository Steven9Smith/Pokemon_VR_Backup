//#define QUICKSEARCH_DEBUG

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.QuickSearch.Providers
{
    [UsedImplicitly]
    public class SceneProvider : SearchProvider
    {
        const int k_LODDetail1 = 50000;
        const int k_LODDetail2 = 100000;

        protected struct GOD
        {
            public string id;
            public string tokens;
            public string keywords;
            public GameObject gameObject;
        }

        protected class SceneSearchIndexer : SearchIndexer
        {
            private const int k_MinIndexCharVariation = 2;
            private const int k_MaxIndexCharVariation = 8;

            private GOD[] gods { get; set; }

            public SceneSearchIndexer(string sceneName, GOD[] gods)
                : base(new []{new Root("", sceneName) })
            {
                this.gods = gods;
                minIndexCharVariation = k_MinIndexCharVariation;
                maxIndexCharVariation = k_MaxIndexCharVariation;
                skipEntryHandler = e => false;
                getEntryComponentsHandler = (e, i) => SplitComponents(e, entrySeparators, k_MaxIndexCharVariation);
                enumerateRootEntriesHandler = EnumerateSceneObjects;
            }

            private IEnumerable<string> EnumerateSceneObjects(Root root)
            {
                return gods.Select(god => god.keywords);
            }

            internal static IEnumerable<string> SplitComponents(string path)
            {
                return SplitComponents(path, SearchUtils.entrySeparators, k_MaxIndexCharVariation);
            }

            private static IEnumerable<string> SplitComponents(string path, char[] entrySeparators, int maxIndexCharVariation)
            {
                var nameTokens = path.Split(entrySeparators).Reverse().ToArray();
                var scc = nameTokens.SelectMany(s => SearchUtils.SplitCamelCase(s)).Where(s => s.Length > 0);
                return nameTokens.Concat(scc)
                          .Select(s => s.Substring(0, Math.Min(s.Length, maxIndexCharVariation)).ToLowerInvariant())
                          .Distinct();
            }
        }

        protected GOD[] gods { get; set; }
        protected SceneSearchIndexer indexer { get; set; }
        protected Dictionary<int, string> componentsById { get; set; } = new Dictionary<int, string>();
        protected Dictionary<int, int> patternMatchCount { get; set; } = new Dictionary<int, int>();
        protected bool m_HierarchyChanged = true;

        protected Func<GameObject[]> fetchGameObjects { get; set; }
        protected Func<GOD, GameObject[], string> buildKeywordComponents { get; set; }

        private IEnumerator<SearchItem> m_GodBuilderEnumerator = null;
        private static readonly Stack<StringBuilder> _SbPool = new Stack<StringBuilder>();

        public SceneProvider(string providerId, string filterId, string displayName)
            : base(providerId, displayName)
        {
            priority = 50;
            this.filterId = filterId;

            subCategories = new List<NameId>
            {
                new NameId("fuzzy", "fuzzy"),
            };

            isEnabledForContextualSearch = () =>
                QuickSearchTool.IsFocusedWindowTypeName("SceneView") ||
                QuickSearchTool.IsFocusedWindowTypeName("SceneHierarchyWindow");

            EditorApplication.hierarchyChanged += () => m_HierarchyChanged = true;

            onEnable = () =>
            {
                if (m_HierarchyChanged)
                {
                    componentsById.Clear();
                    indexer = null;
                    gods = null;
                    m_GodBuilderEnumerator = null;
                    m_HierarchyChanged = false;
                }
            };

            onDisable = () => {};

            fetchItems = (context, items, provider) => FetchItems(context, provider);

            fetchLabel = (item, context) =>
            {
                if (item.label != null)
                    return item.label;

                var go = ObjectFromItem(item);
                if (!go)
                    return item.id;

                var transformPath = GetTransformPath(go.transform);
                var components = go.GetComponents<Component>();
                if (components.Length > 2 && components[1] && components[components.Length-1])
                    item.label = $"{transformPath} ({components[1].GetType().Name}..{components[components.Length-1].GetType().Name})";
                else if (components.Length > 1 && components[1])
                    item.label = $"{transformPath} ({components[1].GetType().Name})";
                else
                    item.label = $"{transformPath} ({item.id})";

                long score = 1;
                List<int> matches = new List<int>();
                var sq = CleanString(context.searchQuery);
                if (FuzzySearch.FuzzyMatch(sq, CleanString(item.label), ref score, matches))
                    item.label = RichTextFormatter.FormatSuggestionTitle(item.label, matches);

                return item.label;
            };

            fetchDescription = (item, context) =>
            {
                #if QUICKSEARCH_DEBUG
                item.description = gods[(int)item.data].name + " * " + item.score;
                #else
                var go = ObjectFromItem(item);
                item.description = GetHierarchyPath(go);
                #endif
                return item.description;
            };

            fetchThumbnail = (item, context) =>
            {
                var obj = ObjectFromItem(item);
                if (obj == null)
                    return null;

                item.thumbnail = PrefabUtility.GetIconForGameObject(obj);
                if (item.thumbnail)
                    return item.thumbnail;
                return EditorGUIUtility.ObjectContent(obj, obj.GetType()).image as Texture2D;
            };

            fetchPreview = (item, context, size, options) =>
            {
                var obj = ObjectFromItem(item);
                if (obj == null)
                    return item.thumbnail;

                var assetPath = GetHierarchyAssetPath(obj, true);
                if (String.IsNullOrEmpty(assetPath))
                    return item.thumbnail;
                return AssetPreview.GetAssetPreview(obj) ?? Utils.GetAssetPreviewFromPath(assetPath);
            };

            startDrag = (item, context) =>
            {
                var obj = ObjectFromItem(item);
                if (obj != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { obj };
                    DragAndDrop.StartDrag("Drag scene object");
                }
            };

            fetchGameObjects = FetchGameObjects;
            buildKeywordComponents = BuildKeywordComponents;

            trackSelection = (item, context) => PingItem(item);
        }

        private IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            if (m_GodBuilderEnumerator == null)
                m_GodBuilderEnumerator = BuildGODS(context, provider);

            while (m_GodBuilderEnumerator.MoveNext())
                yield return m_GodBuilderEnumerator.Current;

            if (indexer == null)
            {
                indexer = new SceneSearchIndexer(SceneManager.GetActiveScene().name, gods)
                {
                    patternMatchCount = patternMatchCount
                };
                indexer.Build();
                yield return null;
            }

            foreach (var item in SearchGODs(context, provider))
                yield return item;

            while (!indexer.IsReady())
                yield return null;

            foreach (var r in indexer.Search(context.searchQuery))
            {
                if (r.index < 0 || r.index >= gods.Length)
                    continue;

                var god = gods[r.index];
                var gameObjectId = god.id;
                var gameObjectName = god.gameObject.name;
                var itemScore = r.score - 1000;
                if (gameObjectName.Equals(context.searchQuery, StringComparison.InvariantCultureIgnoreCase))
                    itemScore *= 2;
                var item = provider.CreateItem(gameObjectId, itemScore, null, null, null, r.index);
                item.descriptionFormat = SearchItemDescriptionFormat.Ellipsis |
                    SearchItemDescriptionFormat.RightToLeft |
                    SearchItemDescriptionFormat.Highlight;
                yield return item;
            }
        }

        private IEnumerable<SearchItem> SearchGODs(SearchContext context, SearchProvider provider)
        {
            List<int> matches = new List<int>();
            var sq = CleanString(context.searchQuery);
            var useFuzzySearch = gods.Length < k_LODDetail2 && context.categories.Any(c => c.name.id == "fuzzy" && c.isEnabled);

            for (int i = 0, end = gods.Length; i != end; ++i)
            {
                var go = gods[i].gameObject;
                if (!go)
                    continue;

                yield return MatchGOD(context, provider, gods[i], i, useFuzzySearch, sq, matches);
            }
        }

        protected GameObject[] FetchGameObjects()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                return SceneModeUtility.GetObjects(new[] { prefabStage.prefabContentsRoot }, true);
            
            var goRoots = new List<UnityEngine.Object>();
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                    continue;

                var sceneRootObjects = scene.GetRootGameObjects();
                if (sceneRootObjects != null && sceneRootObjects.Length > 0)
                    goRoots.AddRange(sceneRootObjects);
            }

            return SceneModeUtility.GetObjects(goRoots.ToArray(), true);
        }

        protected string BuildKeywordComponents(GOD descriptor, GameObject[] objects)
        {
            if (gods.Length > k_LODDetail2)
                return descriptor.gameObject.name;
            if (gods.Length > k_LODDetail1)
                return GetTransformPath(descriptor.gameObject.transform);

            return BuildComponents(descriptor.gameObject);
        }

        private IEnumerator<SearchItem> BuildGODS(SearchContext context, SearchProvider provider)
        {
            var matches = new List<int>();
            var objects = fetchGameObjects();
            var filter = CleanString(context.searchQuery);
            var useFuzzySearch = objects.Length < k_LODDetail2 && context.categories.Any(c => c.name.id == "fuzzy" && c.isEnabled);

            gods = new GOD[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                var id = objects[i].GetInstanceID();
                gods[i] = new GOD{ id = id.ToString(), gameObject = objects[i]};
                if (!componentsById.TryGetValue(id, out gods[i].tokens))
                {
                    gods[i].keywords = buildKeywordComponents(gods[i], objects);
                    componentsById[id] = gods[i].tokens = CleanString(gods[i].keywords);
                }
                yield return MatchGOD(context, provider, gods[i], i, useFuzzySearch, filter, matches);
            }
        }

        private SearchItem MatchGOD(SearchContext context, SearchProvider provider, GOD god, int index, bool useFuzzySearch, string fuzzyMatchQuery, List<int> FuzzyMatches)
        {
            long score = -1;
            if (useFuzzySearch)
            {
                if (!FuzzySearch.FuzzyMatch(fuzzyMatchQuery, god.tokens, ref score, FuzzyMatches))
                    return null;
            }
            else
            {
                if (!MatchSearchGroups(context, god.tokens, true))
                    return null;
            }

            var item = provider.CreateItem(god.id, ~(int)score, null, null, null, index);
            item.descriptionFormat = SearchItemDescriptionFormat.Ellipsis | SearchItemDescriptionFormat.RightToLeft;
            if (useFuzzySearch)
                item.descriptionFormat |= SearchItemDescriptionFormat.FuzzyHighlight;
            else
                item.descriptionFormat |= SearchItemDescriptionFormat.Highlight;
            return item;
        }

        private static string CleanString(string s)
        {
            var sb = s.ToCharArray();
            for (int c = 0; c < s.Length; ++c)
            {
                var ch = s[c];
                if (ch == '_' || ch == '.' || ch == '-' || ch == '/')
                    sb[c] = ' ';
            }
            return new string(sb).ToLowerInvariant();
        }

        private static UnityEngine.Object PingItem(SearchItem item)
        {
            var obj = ObjectFromItem(item);
            if (obj == null)
                return null;
            EditorGUIUtility.PingObject(obj);
            return obj;
        }

        private static void FrameObject(object obj)
        {
            Selection.activeGameObject = obj as GameObject ?? Selection.activeGameObject;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }

        private static GameObject ObjectFromItem(SearchItem item)
        {
            var instanceID = Convert.ToInt32(item.id);
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            return obj;
        }

        private static string GetTransformPath(Transform tform)
        {
            if (tform.parent == null)
                return "/" + tform.name;
            return GetTransformPath(tform.parent) + "/" + tform.name;
        }

        public static string BuildComponents(GameObject go)
        {
            var components = new List<string>();
            var tform = go.transform;
            while (tform != null)
            {
                components.Insert(0, tform.name);
                tform = tform.parent;
            }

            components.Insert(0, go.scene.name);

            var gocs = go.GetComponents<Component>();
            for (int i = 1; i < gocs.Length; ++i)
            {
                var c = gocs[i];
                if (!c || c.hideFlags == HideFlags.HideInInspector)
                    continue;
                components.Add(c.GetType().Name);
            }

            return String.Join(" ", components.Distinct());
        }

        public static string GetHierarchyPath(GameObject gameObject, bool includeScene = true)
        {
            if (gameObject == null)
                return String.Empty;

            StringBuilder sb;
            if (_SbPool.Count > 0)
            {
                sb = _SbPool.Pop();
                sb.Clear();
            }
            else
            {
                sb = new StringBuilder(200);
            }

            try
            {
                if (includeScene)
                {
                    var sceneName = gameObject.scene.name;
                    if (sceneName == string.Empty)
                    {
                        #if UNITY_2018_3_OR_NEWER
                        var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                        if (prefabStage != null)
                        {
                            sceneName = "Prefab Stage";
                        }
                        else
                        #endif
                        {
                            sceneName = "Unsaved Scene";
                        }
                    }

                    sb.Append("<b>" + sceneName + "</b>");
                }

                sb.Append(GetTransformPath(gameObject.transform));

                #if false
                bool isPrefab;
                #if UNITY_2018_3_OR_NEWER
                isPrefab = PrefabUtility.GetPrefabAssetType(gameObject.gameObject) != PrefabAssetType.NotAPrefab;
                #else
                isPrefab = UnityEditor.PrefabUtility.GetPrefabType(o) == UnityEditor.PrefabType.Prefab;
                #endif
                var assetPath = string.Empty;
                if (isPrefab)
                {
                    #if UNITY_2018_3_OR_NEWER
                    assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                    #else
                    assetPath = AssetDatabase.GetAssetPath(gameObject);
                    #endif
                    sb.Append(" (" + System.IO.Path.GetFileName(assetPath) + ")");
                }
                #endif

                var path = sb.ToString();
                sb.Clear();
                return path;
            }
            finally
            {
                _SbPool.Push(sb);
            }
        }

        public static string GetHierarchyAssetPath(GameObject gameObject, bool prefabOnly = false)
        {
            if (gameObject == null)
                return String.Empty;

            bool isPrefab;
            #if UNITY_2018_3_OR_NEWER
            isPrefab = PrefabUtility.GetPrefabAssetType(gameObject.gameObject) != PrefabAssetType.NotAPrefab;
            #else
            isPrefab = UnityEditor.PrefabUtility.GetPrefabType(o) == UnityEditor.PrefabType.Prefab;
            #endif

            var assetPath = string.Empty;
            if (isPrefab)
            {
                #if UNITY_2018_3_OR_NEWER
                assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                #else
                assetPath = AssetDatabase.GetAssetPath(gameObject);
                #endif
                return assetPath;
            }

            if (prefabOnly)
                return null;

            return gameObject.scene.path;
        }

        public static IEnumerable<SearchAction> CreateActionHandlers(string providerId)
        {
            return new SearchAction[]
            {
                new SearchAction(providerId, "select", null, "Select object in scene...")
                {
                    handler = (item, context) =>
                    {
                        var pingedObject = PingItem(item);
                        if (pingedObject != null)
                            FrameObject(pingedObject);
                    }
                },

                new SearchAction(providerId, "open", null, "Open containing asset...")
                {
                    handler = (item, context) =>
                    {
                        var pingedObject = PingItem(item);
                        if (pingedObject != null)
                        {
                            var go = pingedObject as GameObject;
                            var assetPath = GetHierarchyAssetPath(go);
                            if (!String.IsNullOrEmpty(assetPath))
                                Utils.FrameAssetFromPath(assetPath);
                            else
                                FrameObject(go);
                        }
                    }
                }
            };
        }
    }

    static class BuiltInSceneObjectsProvider
    {
        const string k_DefaultProviderId = "scene";

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SceneProvider(k_DefaultProviderId, "h:", "Scene");
        }

        [UsedImplicitly, SearchActionsProvider]
        internal static IEnumerable<SearchAction> ActionHandlers()
        {
            return SceneProvider.CreateActionHandlers(k_DefaultProviderId);
        }

        #if UNITY_2019_1_OR_NEWER
        [UsedImplicitly, Shortcut("Help/Quick Search/Scene")]
        private static void OpenQuickSearch()
        {
            QuickSearchTool.OpenWithContextualProvider(k_DefaultProviderId);
        }
        #endif
    }
}
