using System;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.QuickSearch
{
    internal class QuickSearchToolTests
    {
        private static readonly int[] k_IterationCount = {1, 5, 10};

        [SetUp]
        public void EnableService()
        {
            SearchService.Enable(SearchContext.Empty);
            SearchService.Filter.ResetFilter(true);
            SearchService.Providers.First(p => p.name.id == "packages").active = false;
        }

        [TearDown]
        public void DisableService()
        {
            SearchService.Disable(SearchContext.Empty);
        }

        [UnityTest]
        public IEnumerator Open()
        {
            var qsWindow = QuickSearchTool.ShowWindow();
            Assert.IsNotNull(qsWindow);

            yield return SendKeyCharacterEvent(qsWindow, (char) 0, KeyCode.Escape);

            // The window should have been closed and destroy by then.
            Assert.IsTrue(!qsWindow);
        }

        [UnityTest]
        public IEnumerator Search_Assets([ValueSource(nameof(k_IterationCount))] int iterationCount)
        {
            for (int i = 0; i < iterationCount; ++i)
            {
                var qsWindow = QuickSearchTool.ShowWindow();
                yield return PrepareSearchTool(qsWindow);

                var queryString = "test 42";
                foreach (var c in queryString)
                    yield return SendKeyCharacterEvent(qsWindow, c);

                yield return WaitForSearchCompleted(qsWindow);

                var results = qsWindow.Results.ToArray();
                Assert.GreaterOrEqual(results.Length, 1);
                Assert.IsTrue(results.Any(r => StripHTML(r.label).Contains("test_material_42")));

                yield return SendKeyCharacterEvent(qsWindow, (char) 0, KeyCode.Escape);
            }
        }

        [UnityTest]
        public IEnumerator Search_SceneObjects([ValueSource(nameof(k_IterationCount))] int iterationCount)
        {
            for (int i = 0; i < iterationCount; ++i)
            {
                var hierarchyChanged = false;

                void OnEditorApplicationOnHierarchyChanged() => hierarchyChanged = true;
                EditorApplication.hierarchyChanged += OnEditorApplicationOnHierarchyChanged;

                var uniqueName = GUID.Generate().ToString();
                var go = new GameObject(uniqueName);
                Assert.IsNotNull(go);
                Assert.AreEqual(uniqueName, go.name);

                while (!hierarchyChanged)
                    yield return null;

                EditorApplication.hierarchyChanged -= OnEditorApplicationOnHierarchyChanged;

                var qsWindow = QuickSearchTool.ShowWindow();
                yield return PrepareSearchTool(qsWindow);

                var queryString = uniqueName.Substring(Random.Range(0, uniqueName.Length / 2 - 1), Math.Max(3, Random.Range(0, uniqueName.Length / 2 - 1)));
                Debug.Log($"Searching {queryString} in {uniqueName}");

                yield return SendKeyCharacterEvent(qsWindow, 'h');
                yield return SendKeyCharacterEvent(qsWindow, ':');
                foreach (var c in queryString)
                    yield return SendKeyCharacterEvent(qsWindow, c);

                yield return WaitForSearchCompleted(qsWindow);

                var results = qsWindow.Results.ToArray();
                var searchContext = qsWindow.Context;
                Assert.GreaterOrEqual(results.Length, 1);
                Assert.IsTrue(results.Any(r => StripHTML(r.provider.fetchLabel(r, searchContext)).Contains(uniqueName)));

                yield return SendKeyCharacterEvent(qsWindow, (char) 0, KeyCode.Escape);
            }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        private static IEnumerator WaitForSearchCompleted(QuickSearchTool qsWindow)
        {
            qsWindow.Refresh();
            yield return null;
            while (AsyncSearchSession.SearchInProgress)
                yield return null;
            yield return null;
        }

        private IEnumerator PrepareSearchTool(QuickSearchTool qsWindow)
        {
            qsWindow.m_SendAnalyticsEvent = true;
            Assert.IsNotNull(qsWindow);
            yield return null;

            qsWindow.Focus();
            yield return null;

            yield return SendKeyCharacterEvent(qsWindow, (char)0, KeyCode.Backspace, EventModifiers.FunctionKey);

            qsWindow.Refresh();
            while (AsyncSearchSession.SearchInProgress)
                yield return null;
            yield return null;

            Assert.IsEmpty(qsWindow.Context.searchText);
        }

        private IEnumerator SendKeyCharacterEvent(EditorWindow w, char c, KeyCode keyCode = KeyCode.None, EventModifiers modifiers = EventModifiers.None)
        {
            w.SendEvent(new Event { type = EventType.KeyDown, character = c, keyCode = keyCode, modifiers = modifiers});
            yield return null;

            if (w)
            {
                w.SendEvent(new Event { type = EventType.KeyUp, character = c, keyCode = keyCode, modifiers = modifiers });
                yield return null;
            }
        }
    }
}
