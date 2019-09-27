//#define QUICKSEARCH_DEBUG
using System.Collections.Generic;
using System.IO;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        public class FileSearchIndexer : SearchIndexer
        {
            private const int k_MinIndexCharVariation = 2;
            private const int k_MaxIndexCharVariation = 12;

            public string type { get; }

            public FileSearchIndexer(string type, IEnumerable<Root> roots)
                : base (roots)
            {
                this.type = type;
                minIndexCharVariation = k_MinIndexCharVariation;
                maxIndexCharVariation = k_MaxIndexCharVariation;
                skipEntryHandler = ShouldSkipEntry;
                getIndexFilePathHandler = GetIndexFilePath;
                getEntryComponentsHandler = (e, i) => SearchUtils.SplitFileEntryComponents(e, entrySeparators, k_MinIndexCharVariation, k_MaxIndexCharVariation);
                enumerateRootEntriesHandler = EnumerateAssetPaths;

                SearchService.contentRefreshed += UpdateIndexWithNewContent;
            }

            private static bool ShouldSkipEntry(string entry)
            {
                return entry.Length == 0 || entry[0] == '.' || entry.EndsWith(".meta");
            }

            private string GetIndexFilePath(string basePath)
            {
                string indexFileName = $"quicksearch.{type}.index";
                return Path.GetFullPath(Path.Combine(basePath, "..", "Library", indexFileName));
            }

            private static IEnumerable<string> EnumerateAssetPaths(Root root)
            {
                return Directory.EnumerateFiles(root.basePath, "*.*", SearchOption.AllDirectories);
            }
        }
    }
}
