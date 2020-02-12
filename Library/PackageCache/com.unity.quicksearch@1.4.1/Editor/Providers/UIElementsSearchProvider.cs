﻿
#if UNITY_2020_1_OR_NEWER
using System.Reflection;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Unity.QuickSearch.Providers
{
    class VisualElementInfo
    {
        public string hostWindowName;
        public EditorWindow window;
        public VisualElement element;
    }

    [UsedImplicitly]
    static class UIElementsSearchProvider
    {
        const string type = "ui_elements";
        const string displayName = "UI Elements";
        const string filterId = "uie:";

        private static EditorWindow[] s_AllEditorWindows = new EditorWindow[0];

        [UsedImplicitly, SearchItemProvider]
        private static SearchProvider CreateProvider()
        {
            return new SearchProvider(type, displayName)
            {
                active = false,
                priority = 115,
                filterId = filterId,

                onEnable = () =>
                {
                    s_AllEditorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                },

                onDisable = () =>
                {
                    s_AllEditorWindows = new EditorWindow[0];
                },

                fetchItems = (context, items, provider) => FetchItems(context, provider),

                fetchLabel = (item, context) =>
                {
                    var info = (VisualElementInfo)item.data;
                    var element = info.element;
                    var visibleLabel = element.visible ? "Visible" : "Hidden";
                    return $"<b>{info.hostWindowName}</b>/{GetName(element)} ({visibleLabel})";
                },

                fetchDescription = (item, context) =>
                {
                    var info = (VisualElementInfo)item.data;
                    var element = info.element;
                    return $"{GetTransformPath(element)}";
                },

                fetchThumbnail = (item, context) => Icons.quicksearch,

                trackSelection = (item, context) =>
                {
                    var info = (VisualElementInfo)item.data;
                    var element = info.element;
                    var s = element.style;
                    var oldBorderTopColor = s.borderTopColor;
                    var oldBorderBottomColor = s.borderBottomColor;
                    var oldBorderLeftColor = s.borderLeftColor;
                    var oldBorderRightColor = s.borderRightColor;
                    var oldBorderTopWidth = s.borderTopWidth;
                    var oldBorderBottomWidth = s.borderBottomWidth;
                    var oldBorderLeftWidth = s.borderLeftWidth;
                    var oldBorderRightWidth = s.borderRightWidth;

                    s.borderTopWidth = s.borderBottomWidth = s.borderLeftWidth = s.borderRightWidth = new StyleFloat(2);
                    s.borderTopColor = s.borderBottomColor = s.borderLeftColor = s.borderRightColor = new StyleColor(Color.cyan);

                    element.Focus();

                    DelayCall(1f, () =>
                    {
                        s.borderTopColor = oldBorderTopColor;
                        s.borderBottomColor = oldBorderBottomColor;
                        s.borderLeftColor = oldBorderLeftColor;
                        s.borderRightColor = oldBorderRightColor;
                        s.borderTopWidth = oldBorderTopWidth;
                        s.borderBottomWidth = oldBorderBottomWidth;
                        s.borderLeftWidth = oldBorderLeftWidth;
                        s.borderRightWidth = oldBorderRightWidth;

                        if (info.window)
                            info.window.Repaint();
                    });
                }
            };
        }

        [UsedImplicitly, SearchActionsProvider]
        private static IEnumerable<SearchAction> ActionHandlers()
        {
            return new[]
            {
                new SearchAction(type, "select", null, "Select visual element...")
                {
                    handler = (item, context) =>
                    {
                        var element = ((VisualElementInfo)item.data).element;
                        var oldBackgroundColor = element.style.backgroundColor;
                        element.style.backgroundColor = new StyleColor(Color.green);
                        element.Focus();
                        DelayCall(2f, () => element.style.backgroundColor = oldBackgroundColor);
                    }
                },
                new SearchAction(type, "inspect", null, "Inspect visual element...")
                {
                    handler = (item, context) => InspectElement(((VisualElementInfo)item.data).element)
                }
            };
        }

        private static void DelayCall(float seconds, System.Action callback)
        {
            DelayCall(EditorApplication.timeSinceStartup, seconds, callback);
        }

        private static void DelayCall(double timeStart, float seconds, System.Action callback)
        {
            var dt = EditorApplication.timeSinceStartup - timeStart;
            if (dt >= seconds)
                callback();
            else
                EditorApplication.delayCall += () => DelayCall(timeStart, seconds, callback);
        }

        private static string GetName(VisualElement elm)
        {
            var name = elm.name;
            if (String.IsNullOrEmpty(name))
                name = elm.GetType().Name;
            return name;
        }

        private static string GetTransformPath(VisualElement elm)
        {
            var name = GetName(elm);
            if (elm.parent == null)
                return $"/{name}";
            return $"{GetTransformPath(elm.parent)}/{name}";
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            // Fetch all editor windows
            foreach (var win in s_AllEditorWindows)
            {
                if (!win)
                    continue;

                // Query the UIElements DOM
                var allElements = win.rootVisualElement.Query("");
                foreach (var element in allElements.ToList())
                {
                    // Check if match
                    if (SearchProvider.MatchSearchGroups(context, GetName(element)) ||
                        SearchProvider.MatchSearchGroups(context, element.tooltip) ||
                        SearchProvider.MatchSearchGroups(context, GetTransformPath(element)) ||
                        (element is TextElement textElement && SearchProvider.MatchSearchGroups(context, textElement.text)))
                    {
                        // Return matching elements
                        var info = new VisualElementInfo { window = win, element = element, hostWindowName = win.titleContent.text };
                        var item = provider.CreateItem($"uie_{element.GetHashCode()}", element.tabIndex, null, null, null, info);
                        item.descriptionFormat |= SearchItemDescriptionFormat.Ellipsis | SearchItemDescriptionFormat.RightToLeft | SearchItemDescriptionFormat.FuzzyHighlight;
                        yield return item;
                    }
                    else
                        yield return null;
                }
            }
        }

        private static void InspectElement(VisualElement element)
        {
            var typeCollection = TypeCache.GetTypesDerivedFrom<EditorWindow>();
            foreach (var tew in typeCollection)
            {
                if (tew.Name != "UIElementsDebugger")
                    continue;

                var uiDebuggerWindow = EditorWindow.GetWindow(tew);
                tew.InvokeMember("SelectElement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, uiDebuggerWindow, new object[] { element });
                break;
            }
        }
    }
}
#endif  //UNITY_2019_3_OR_NEWER
