using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.QuickSearch.Providers;
using UnityEngine;

namespace Unity.QuickSearch
{
    class SearchIndexerTests
    {
        [SetUp]
        public void EnableService()
        {
            SearchService.Enable(SearchContext.Empty);
            SearchService.Filter.ResetFilter(true);
        }

        [TearDown]
        public void DisableService()
        {
            SearchService.Disable(SearchContext.Empty);
        }

        [Test]
        public void IndexSorting()
        {
            List<SearchIndexer.WordIndexEntry> indexedWords = new List<SearchIndexer.WordIndexEntry>()
            {
                new SearchIndexer.WordIndexEntry(33, 2, 1, 2),
                new SearchIndexer.WordIndexEntry(33, 2, 1, 44),
                new SearchIndexer.WordIndexEntry(33, 3, 1, 445),
                new SearchIndexer.WordIndexEntry(33, 1, 2, 446),
                new SearchIndexer.WordIndexEntry(33, 2, 1, 1),
                new SearchIndexer.WordIndexEntry(34, 3, 2, 447),
                new SearchIndexer.WordIndexEntry(33, 1, 2, -3)
            };

            Assert.AreEqual(indexedWords.Count, 7);

            Debug.Log($"===> Raw {indexedWords.Count}");
            foreach (var w in indexedWords)
                Debug.Log($"Word {w.length} - {w.key} - {w.fileIndex} - {w.score}");

            indexedWords.Sort(SearchIndexer.SortWordEntryComparer);
            Debug.Log($"===> Sort {indexedWords.Count}");
            foreach (var w in indexedWords)
                Debug.Log($"Word {w.length} - {w.key} - {w.fileIndex} - {w.score}");

            Assert.AreEqual(indexedWords.Count, 7);
            Assert.AreEqual(ToString(indexedWords[0]), "1 - 33 - 2 - -3");
            Assert.AreEqual(ToString(indexedWords[1]), "1 - 33 - 2 - 446");
            Assert.AreEqual(ToString(indexedWords[2]), "2 - 33 - 1 - 1");
            Assert.AreEqual(ToString(indexedWords[3]), "2 - 33 - 1 - 2");
            Assert.AreEqual(ToString(indexedWords[4]), "2 - 33 - 1 - 44");
            Assert.AreEqual(ToString(indexedWords[5]), "3 - 33 - 1 - 445");
            Assert.AreEqual(ToString(indexedWords[6]), "3 - 34 - 2 - 447");

            indexedWords = indexedWords.Distinct().ToList();
            Debug.Log($"===> Distinct {indexedWords.Count}");
            foreach (var w in indexedWords)
                Debug.Log($"Word {w.length} - {w.key} - {w.fileIndex} - {w.score}");

            Assert.AreEqual(indexedWords.Count, 4);
            Assert.AreEqual(ToString(indexedWords[0]), "1 - 33 - 2 - -3");
            Assert.AreEqual(ToString(indexedWords[1]), "2 - 33 - 1 - 1");
            Assert.AreEqual(ToString(indexedWords[2]), "3 - 33 - 1 - 445");
            Assert.AreEqual(ToString(indexedWords[3]), "3 - 34 - 2 - 447");
        }

        private static string ToString(SearchIndexer.WordIndexEntry w)
        {
            return $"{w.length} - {w.key} - {w.fileIndex} - {w.score}";
        }
    }
}
