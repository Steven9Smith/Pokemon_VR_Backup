using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class AssetStoreList
        {
            internal static readonly string type = "asset_store";
            internal static readonly string displayName = "Asset Store";
            internal static readonly string url = "http://shawarma.unity3d.com/public-api/search/assets.json?unityversion=2019.2.0a7&skip_terms=1&q=";

            class AssetStoreData
            {
                public int id;
                public int packageId;
            }
            class AssetPreviewData
            {
                public string staticPreviewUrl;
                public Texture2D preview;
                public UnityWebRequest request;
                public UnityWebRequestAsyncOperation requestOp;
            }

            private static Dictionary<string, AssetPreviewData> s_CurrentSearchAssetData = new Dictionary<string, AssetPreviewData>();

            [UsedImplicitly, SearchItemProvider]
            internal static SearchProvider CreateProvider()
            {
                return new SearchProvider(type, displayName)
                {
                    active = false, // Still experimental
                    priority = 120,
                    filterId = "store:",
                    fetchItems = (context, items, provider) => SearchItems(context, provider),

                    fetchThumbnail = (item, context) => Icons.store,

                    fetchPreview = (item, context, size, options) =>
                    {
                        if (!s_CurrentSearchAssetData.ContainsKey(item.id)) 
                            return null;

                        var searchPreview = s_CurrentSearchAssetData[item.id];
                        if (searchPreview.preview)
                        {
                            return s_CurrentSearchAssetData[item.id].preview;
                        }

                        if (searchPreview.request == null)
                        {
                            searchPreview.request = UnityWebRequestTexture.GetTexture(searchPreview.staticPreviewUrl);
                            searchPreview.requestOp = searchPreview.request.SendWebRequest();
                            searchPreview.requestOp.completed += TextureFetched;
                        }

                        return null;
                    },

                    onDisable = () =>
                    {
                        foreach(var sp in s_CurrentSearchAssetData.Values)
                        {
                            if (sp.preview)
                            {
                                UnityEngine.Object.DestroyImmediate(sp.preview);
                            }
                            sp.request = null;
                            sp.requestOp = null;
                        }
                        s_CurrentSearchAssetData.Clear();
                    }

                };
            }

            [UsedImplicitly, SearchActionsProvider]
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction(type, "open", null, "Open package in the Asset Store...") {
                        handler = (item, context) =>
                        {
                            var data = (AssetStoreData)item.data;
                            AssetStore.Open(string.Format("content/{0}?assetID={1}", data.packageId, data.id));
                        }
                    }
                };
            }

            internal static void TextureFetched(AsyncOperation op)
            {
                var searchPreview = s_CurrentSearchAssetData.Values.FirstOrDefault(sp => sp.requestOp == op);
                if (searchPreview != null)
                {
                    if (searchPreview.request.isDone && !searchPreview.request.isHttpError && !searchPreview.request.isNetworkError)
                    {
                        searchPreview.preview = DownloadHandlerTexture.GetContent(searchPreview.request);
                    }
                    searchPreview.requestOp.completed -= TextureFetched;
                    searchPreview.requestOp = null;
                }
            }

            internal static IEnumerable<SearchItem> SearchItems(SearchContext context, SearchProvider provider)
            {
                var webRequest = UnityWebRequest.Get(url + context.searchQuery);
                webRequest.SetRequestHeader("X-Unity-Session", InternalEditorUtility.GetAuthToken());
                webRequest.SendWebRequest(); // Don't yield return this, as it is not a coroutine and will block the UI

                while (!webRequest.isDone)
                {
                    if (webRequest.isHttpError || webRequest.isNetworkError)
                        yield break;
                    yield return null;
                }

                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    yield break;


                var reqJson = Utils.JsonDeserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                if (reqJson == null || !reqJson.ContainsKey("status") || reqJson["status"].ToString() != "ok" || !reqJson.ContainsKey("groups"))
                {
                    yield break;
                }

                if (!(reqJson["groups"] is List<object> groups))
                    yield break;

                foreach (var g in groups)
                {
                    var group = g as Dictionary<string, object>;
                    if (group == null || !group.ContainsKey("matches"))
                    {
                        yield return null;
                        continue;
                    }

                    var matches = group["matches"] as List<object>;
                    if (matches == null)
                    {
                        yield return null;
                        continue;
                    }

                    foreach (var m in matches.Take(50))
                    {
                        var match = m as Dictionary<string, object>;
                        if (match == null)
                        {
                            yield return null;
                            continue;
                        }
                        match["groupType"] = group["name"];

                        if (!match.ContainsKey("name") || !match.ContainsKey("id") || !match.ContainsKey("package_id"))
                        {
                            yield return null;
                            continue;
                        }
                        var id = match["id"].ToString();
                        var data = new AssetStoreData {packageId = Convert.ToInt32(match["package_id"]), id = Convert.ToInt32(match["id"])};
                        var item = provider.CreateItem(id, match["name"].ToString(), $"Asset Store ({match["groupType"]})", null, data);

                        if (match.ContainsKey("static_preview_url") && !s_CurrentSearchAssetData.ContainsKey(id))
                        {
                            s_CurrentSearchAssetData.Add(id, new AssetPreviewData() { staticPreviewUrl = match["static_preview_url"].ToString() });
                        }

                        yield return item;
                    }
                }
            }
        }
    }
}
