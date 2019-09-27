using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.QuickSearch
{
    class AsyncSearchProvider : SearchProvider
    {
        public const int NumberSearchItems = 10;
        public const int NumberNullItems = 10;

        public bool UseSleep { get; set; }
        public int SleepTimeMS { get; set; }

        public AsyncSearchProvider(string id, string displayName = null)
            : base(id, displayName)
        {
            fetchItems = (context, items, provider) => FetchItems(provider);
            UseSleep = false;
            SleepTimeMS = 0;
        }

        private IEnumerable<SearchItem> FetchItems(SearchProvider provider)
        {
            for (var i = 0; i < NumberNullItems; ++i)
            {
                if (UseSleep)
                    Thread.Sleep(SleepTimeMS);
                yield return null;
            }
            for (var i = 0; i < NumberSearchItems; ++i)
            {
                if (UseSleep)
                    Thread.Sleep(SleepTimeMS);
                var item = provider.CreateItem(i.ToString(), i.ToString());
                yield return item;
            }
        }
    }

    internal class SearchServiceTests
    {
        const string k_TestFileName = "Packages/com.unity.quicksearch/Tests/Editor/Content/test_material_42.mat";

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
        public IEnumerator FetchItems()
        {
            var ctx = new SearchContext {searchText = "test_material_42", wantsMore = true};

            var fetchedItems = SearchService.GetItems(ctx);
            while (AsyncSearchSession.SearchInProgress)
                yield return null;

            Assert.IsNotEmpty(fetchedItems);
            var foundItem = fetchedItems.Find(item => item.label == Path.GetFileName(k_TestFileName));
            Assert.IsNotNull(foundItem);
            Assert.IsNotNull(foundItem.id);
            Assert.IsNull(foundItem.description);

            Assert.IsNotNull(foundItem.provider);
            Assert.IsNotNull(foundItem.provider.fetchDescription);
            var fetchedDescription = foundItem.provider.fetchDescription(foundItem, ctx);
            Assert.AreEqual("Packages/com.unity.quicksearch/Tests/Editor/Content/test_material_42.mat (2.0 KB)", fetchedDescription);
        }

        [UnityTest]
        public IEnumerator FetchItemsAsync()
        {
            const string k_SearchType = "___test_async";
            var ctx = new SearchContext { searchText = k_SearchType+":async", wantsMore = true };

            bool wasCalled = false;

            void OnAsyncCallback(IEnumerable<SearchItem> items)
            {
                wasCalled = items.Any(i => i.id == k_SearchType);
            }

            IEnumerable<SearchItem> TestFetchAsyncCallback(SearchProvider provider)
            {
                var ft = UnityEditor.EditorApplication.timeSinceStartup + 1;
                while (UnityEditor.EditorApplication.timeSinceStartup < ft)
                    yield return null;

                yield return provider.CreateItem(k_SearchType);
            }

            var testSearchProvider = new SearchProvider(k_SearchType, k_SearchType)
            {
                filterId = k_SearchType+":",
                isExplicitProvider = true,
                fetchItems = (context, items, provider) => TestFetchAsyncCallback(provider)
            };

            AsyncSearchSession.asyncItemReceived += OnAsyncCallback;

            SearchService.Providers.Add(testSearchProvider);
            SearchService.RefreshProviders();

            SearchService.GetItems(ctx);
            yield return null;

            while (AsyncSearchSession.SearchInProgress)
                yield return null;

            while (!wasCalled)
                yield return null;

            Assert.True(wasCalled);

            SearchService.Providers.Remove(testSearchProvider);
            SearchService.RefreshProviders();
            AsyncSearchSession.asyncItemReceived -= OnAsyncCallback;
        }
    }

    internal class AsyncSearchSessionTests
    {
        AsyncSearchProvider m_Provider;

        [SetUp]
        public void EnableService()
        {
            SearchService.Enable(SearchContext.Empty);
            SearchService.Filter.ResetFilter(true);
            m_Provider = new AsyncSearchProvider(Guid.NewGuid().ToString());
        }

        [TearDown]
        public void DisableService()
        {
            SearchService.Disable(SearchContext.Empty);
        }

        [Test]
        public void FetchSome()
        {
            var session = new AsyncSearchSession();
            var ctx = SearchContext.Empty;
            var items = new List<SearchItem>();
            var enumerable = m_Provider.fetchItems(ctx, items, m_Provider);
            Assert.IsEmpty(items);
            session.Reset(enumerable.GetEnumerator());

            // Test fetching all objects
            var total = AsyncSearchProvider.NumberNullItems + AsyncSearchProvider.NumberSearchItems;
            session.FetchSome(items, total, false);
            Assert.AreEqual(AsyncSearchProvider.NumberSearchItems, items.Count);

            items = new List<SearchItem>();
            session.FetchSome(items, total, false);
            Assert.AreEqual(0, items.Count); // Should be empty since enumerator is at the end

            session.Reset(enumerable.GetEnumerator());
            session.FetchSome(items, AsyncSearchProvider.NumberSearchItems, false);
            Assert.AreEqual(AsyncSearchProvider.NumberSearchItems - AsyncSearchProvider.NumberNullItems, items.Count);

            // Test fetching non-null objects
            session.Reset(enumerable.GetEnumerator());
            session.FetchSome(items, total, true);
            Assert.AreEqual(AsyncSearchProvider.NumberSearchItems, items.Count);

            items = new List<SearchItem>();
            session.FetchSome(items, total, false);
            Assert.AreEqual(0, items.Count); // Should be empty since enumerator is at the end

            session.Reset(enumerable.GetEnumerator());
            session.FetchSome(items, AsyncSearchProvider.NumberSearchItems, true);
            Assert.AreEqual(AsyncSearchProvider.NumberSearchItems, items.Count);

            // Fetch items time constrained
            var maxFetchTimeMs = 1;
            items = new List<SearchItem>();
            m_Provider.UseSleep = true;
            m_Provider.SleepTimeMS = 5;
            session.Reset(enumerable.GetEnumerator());
            session.FetchSome(items, maxFetchTimeMs);
            Assert.AreEqual(0, items.Count);

            // Fetch items with time and size constraints
            items = new List<SearchItem>();
            session.Reset(enumerable.GetEnumerator());
            m_Provider.SleepTimeMS = 1;
            maxFetchTimeMs = m_Provider.SleepTimeMS * AsyncSearchProvider.NumberNullItems;
            session.FetchSome(items, AsyncSearchProvider.NumberSearchItems, true, maxFetchTimeMs);
            Assert.Less(items.Count, AsyncSearchProvider.NumberSearchItems);
        }
    }
}
