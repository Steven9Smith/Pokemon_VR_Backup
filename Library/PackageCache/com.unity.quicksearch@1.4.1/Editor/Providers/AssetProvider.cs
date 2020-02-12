//#define QUICKSEARCH_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        /* Filters:
            t:<type>
            l:<label>
            ref[:id]:path
            v:<versionState>
            s:<softLockState>
            a:<area> [assets, packages]
         */
        [UsedImplicitly]
        static class AssetProvider
        {
            private const string k_ProjectAssetsType = "asset";
            private const string displayName = "Asset";

            private static readonly string[] baseTypeFilters = new[]
            {
                "Folder",
                "DefaultAsset",
                "AnimationClip",
                "AudioClip",
                "AudioMixer",
                "ComputeShader",
                "Font",
                "GUISKin",
                "Material",
                "Mesh",
                "Model",
                "PhysicMaterial",
                "Prefab",
                "Scene",
                "Script",
                "ScriptableObject",
                "Shader",
                "Sprite",
                "StyleSheet",
                "Texture",
                "VideoClip"
            };

            #if UNITY_2019_2_OR_NEWER
            private static readonly string[] typeFilter = baseTypeFilters.Concat(TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                                                                                          .Select(t => t.Name)
                                                                                          .Distinct()
                                                                                          .OrderBy(n => n)).ToArray();
            #else
            private static readonly string[] typeFilter = baseTypeFilters;
            #endif

            private static readonly char[] k_InvalidIndexedChars = { '*', ':' };
            private static readonly char[] k_InvalidSearchFileChars = Path.GetInvalidFileNameChars().Where(c => c != '*').ToArray();

            #region Provider

            private static SearchProvider CreateProvider(string type, string label, string filterId, int priority, GetItemsHandler fetchItemsHandler)
            {
                return new SearchProvider(type, label)
                {
                    priority = priority,
                    filterId = filterId,

                    isEnabledForContextualSearch = () => QuickSearchTool.IsFocusedWindowTypeName("ProjectBrowser"),

                    fetchItems = fetchItemsHandler,

                    fetchKeywords = (context, lastToken, items) =>
                    {
                        if (!lastToken.StartsWith("t:"))
                            return;
                        items.AddRange(typeFilter.Select(t => "t:" + t));
                    },

                    fetchDescription = (item, context) =>
                    {
                        if (AssetDatabase.IsValidFolder(item.id))
                            return item.id;
                        long fileSize = new FileInfo(item.id).Length;
                        item.description = $"{item.id} ({EditorUtility.FormatBytes(fileSize)})";

                        return item.description;
                    },

                    fetchThumbnail = (item, context) => Utils.GetAssetThumbnailFromPath(item.id),
                    fetchPreview = (item, context, size, options) => Utils.GetAssetPreviewFromPath(item.id),

                    startDrag = (item, context) =>
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                        if (obj == null) 
                            return;

                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new[] { obj };
                        DragAndDrop.StartDrag(item.label);
                    },

                    trackSelection = (item, context) =>
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                        if (asset != null)
                            EditorGUIUtility.PingObject(asset);
                    },

                    subCategories = new List<NameId>()
                };
            }

            private static IEnumerable<SearchIndexer.Root> GetPackageRoots()
            {
                return Utils.GetPackagesPaths().Select(p => new SearchIndexer.Root(Path.GetFullPath(p).Replace('\\', '/'), p));
            }

            [UsedImplicitly, SearchItemProvider]
            private  static SearchProvider CreateProjectAssetsProvider()
            {
                var roots = new[] { new SearchIndexer.Root(Application.dataPath, "Assets") }.Concat(GetPackageRoots());
                var fileIndexer = new FileSearchIndexer(k_ProjectAssetsType, roots);
                fileIndexer.Build();

                var provider = CreateProvider(k_ProjectAssetsType, displayName, "p:", 25,
                    (context, items, _provider) => SearchAssets(context, items, _provider, fileIndexer));

                provider.subCategories.Add(new NameId("guid", "guid"));
                provider.subCategories.Add(new NameId("packages", "packages"));

                return provider;
            }

            private static IEnumerable<SearchItem> SearchAssets(SearchContext context, List<SearchItem> items, SearchProvider provider, SearchIndexer fileIndexer)
            {
                var filter = context.searchQuery;
                var searchPackages = context.categories.Any(c => c.name.id == "packages" && c.isEnabled);
                var searchGuids = context.categories.Any(c => c.name.id == "guid" && c.isEnabled);

                if (searchGuids)
                {
                    var gui2Path = AssetDatabase.GUIDToAssetPath(filter);
                    if (!System.String.IsNullOrEmpty(gui2Path))
                    {
                        items.Add(provider.CreateItem(gui2Path, -1, $"{Path.GetFileName(gui2Path)} ({filter})", null, null, null));
                    }
                }

                var indexReady = fileIndexer.IsReady();
                if (indexReady)
                {
                    if (filter.IndexOfAny(k_InvalidIndexedChars) == -1)
                    {
                        foreach (var item in SearchIndex(fileIndexer, filter, searchPackages, provider))
                            yield return item;
                        if (!context.wantsMore)
                            yield break;
                    }
                }

                if (!searchPackages)
                {
                    if (!filter.Contains("a:assets"))
                        filter = "a:assets " + filter;
                }

                foreach (var assetEntry in AssetDatabase.FindAssets(filter).Select(AssetDatabase.GUIDToAssetPath).Select(path => provider.CreateItem(path, Path.GetFileName(path))))
                    yield return assetEntry;

                if (context.searchQuery.Contains('*'))
                {
                    var safeFilter = string.Join("_", context.searchQuery.Split(k_InvalidSearchFileChars));
                    foreach (var fileEntry in Directory.EnumerateFiles(Application.dataPath, safeFilter, SearchOption.AllDirectories)
                                            .Select(path => provider.CreateItem(path.Replace(Application.dataPath, "Assets").Replace("\\", "/"),
                                                                                Path.GetFileName(path))))
                        yield return fileEntry;
                }

                if (!indexReady)
                {
                    // Indicate to the user that we are still building the index.
                    while (!fileIndexer.IsReady())
                        yield return null;

                    foreach (var item in SearchIndex(fileIndexer, filter, searchPackages, provider))
                        yield return item;
                }
            }

            private static IEnumerable<SearchItem> SearchIndex(SearchIndexer fileIndexer, string filter, bool searchPackages, SearchProvider provider)
            {
                return fileIndexer.Search(filter, searchPackages ? int.MaxValue : 100).Select(e => {
                    var filename = Path.GetFileName(e.path);
                    var filenameNoExt = Path.GetFileNameWithoutExtension(e.path);
                    var itemScore = e.score;
                    if (filenameNoExt.Equals(filter, StringComparison.InvariantCultureIgnoreCase))
                        itemScore = SearchProvider.k_RecentUserScore+1;
                    return provider.CreateItem(e.path, itemScore, filename, null, null, null);
                });
            }

            #endregion

            #region Actions

            [UsedImplicitly, SearchActionsProvider]
            private  static IEnumerable<SearchAction> ActionHandlers()
            {
                return CreateActionHandlers(k_ProjectAssetsType);
            }

            private static IEnumerable<SearchAction> CreateActionHandlers(string type)
            {
                #if UNITY_EDITOR_OSX
                const string k_RevealActionLabel = "Reveal in Finder...";
                #else
                const string k_RevealActionLabel = "Show in Explorer...";
                #endif

                return new[]
                {
                    new SearchAction(type, "select", null, "Select asset...")
                    {
                        handler = (item, context) => Utils.FrameAssetFromPath(item.id)
                    },
                    new SearchAction(type, "open", null, "Open asset...")
                    {
                        handler = (item, context) =>
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                            if (asset != null) AssetDatabase.OpenAsset(asset);
                        }
                    },
                    new SearchAction(type, "reveal", null, k_RevealActionLabel)
                    {
                        handler = (item, context) => EditorUtility.RevealInFinder(item.id)
                    },
                    new SearchAction(type, "context", null, "Context")
                    {
                        handler = (item, context) =>
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                            if (asset != null)
                            {
                                Selection.activeObject = asset;
                                EditorUtility.DisplayPopupMenu(QuickSearchTool.ContextualActionPosition, "Assets/", null);
                            }
                        }
                    }
                };
            }

            #endregion

            [UsedImplicitly]
            private class AssetRefreshWatcher : AssetPostprocessor
            {
                private static bool s_Enabled;
                static AssetRefreshWatcher()
                {
                    EditorApplication.update += Enable;
                    EditorApplication.quitting += OnQuitting;
                }

                private static void Enable()
                {
                    EditorApplication.update -= Enable;
                    s_Enabled = true;
                }

                private static void OnQuitting()
                {
                    s_Enabled = false;
                }

                [UsedImplicitly]
                static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
                {
                    if (!s_Enabled)
                        return;

                    SearchService.RaiseContentRefreshed(importedAssets, deletedAssets.Concat(movedFromAssetPaths).Distinct().ToArray(), movedAssets);
                }
            }

            #if UNITY_2019_1_OR_NEWER
            [UsedImplicitly, Shortcut("Help/Quick Search/Assets")]
            private static void PopQuickSearch()
            {
                QuickSearchTool.OpenWithContextualProvider(k_ProjectAssetsType);
            }
            #endif
        }
    }
}
