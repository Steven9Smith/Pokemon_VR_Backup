//#define QUICKSEARCH_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        public class SearchIndexer
        {
            [Serializable, DebuggerDisplay("{key} - {length} - {fileIndex}")]
            internal struct WordIndexEntry
            {
                public readonly int key;
                public readonly int length;
                public readonly int fileIndex;
                public readonly int score;

                public WordIndexEntry(int _key, int _length, int _fileIndex = -1, int _score = int.MaxValue)
                {
                    key = _key;
                    length = _length;
                    fileIndex = _fileIndex;
                    score = _score;
                }

                public override int GetHashCode()
                {
                    return key.GetHashCode() ^ length.GetHashCode() ^ fileIndex.GetHashCode();
                }

                public override bool Equals(object y)
                {
                    WordIndexEntry other = (WordIndexEntry)y;
                    return key == other.key && length == other.length && fileIndex == other.fileIndex;
                }
            }

            [DebuggerDisplay("{path} ({score})")]
            public struct EntryResult
            {
                public string path;
                public int index;
                public int score;
            }

            [DebuggerDisplay("{index} ({score})")]
            private struct PatternMatch
            {
                public PatternMatch(int _i, int _s)
                {
                    index = _i;
                    score = _s;
                }

                public override int GetHashCode()
                {
                    return index.GetHashCode();
                }

                public override bool Equals(object y)
                {
                    PatternMatch other = (PatternMatch)y;
                    return other.index == index;
                }

                public readonly int index;
                public int score;
            }

            [DebuggerDisplay("{baseName} ({basePath})")]
            public struct Root
            {
                public readonly string basePath;
                public readonly string baseName;

                public Root(string _p, string _n)
                {
                    basePath = _p.Replace('\\', '/');
                    baseName = _n;
                }
            }

            public delegate bool SkipEntryHandler(string entry);
            public delegate string[] GetQueryTokensHandler(string query);
            public delegate IEnumerable<string> GetEntryComponentsHandler(string entry, int index);
            public delegate string GetIndexFilePathHandler(string basePath);
            public delegate IEnumerable<string> EnumerateRootEntriesHandler(Root root);

            public Root[] roots { get; }
            public int minIndexCharVariation { get; set; } = 2;
            public int maxIndexCharVariation { get; set; } = 8;
            public char[] entrySeparators { get; set; } = SearchUtils.entrySeparators;

            // Handler used to skip some entries. 
            public SkipEntryHandler skipEntryHandler { get; set; } = e => false;
            
            // Handler used to specify where the index database file should be saved. If the handler returns null, the database won't be saved at all.
            public GetIndexFilePathHandler getIndexFilePathHandler { get; set; } = (p) => null;
            
            // Handler used to parse and split the search query text into words. The tokens needs to be split similarly to how GetEntryComponentsHandler was specified.
            public GetQueryTokensHandler getQueryTokensHandler { get; set; }

            // Handler used to split into words the entries. The order of the words matter. Words at the beginning of the array have a lower score (lower the better)
            public GetEntryComponentsHandler getEntryComponentsHandler { get; set; } = (e, i) => throw new Exception("You need to specify the get entry components handler");

            // Handler used to fetch all the entries under a given root.
            public EnumerateRootEntriesHandler enumerateRootEntriesHandler { get; set; } = r => throw new Exception("You need to specify the root entries enumerator");

            private Thread m_IndexerThread;
            private volatile bool m_IndexReady = false;
            private volatile bool m_ThreadAborted = false;

            private string m_IndexTempFilePath;
            private string[] m_Entries;
            private WordIndexEntry[] m_WordIndexEntries;
            internal Dictionary<int, int> patternMatchCount { get; set; } = new Dictionary<int, int>();

            // 1- Initial format
            // 2- Added score to words
            // 3- Save base name in entry paths
            private const int k_IndexFileVersion = 0x4242E000 | 0x003;

            public SearchIndexer(string rootPath)
                : this(rootPath, String.Empty)
            {
            }

            public SearchIndexer(string rootPath, string rootName)
                : this(new[] { new Root(rootPath, rootName) })
            {
            }

            public SearchIndexer(IEnumerable<Root> roots)
            {
                this.roots = roots.ToArray();
                m_IndexTempFilePath = FileUtil.GetUniqueTempPathInProject();
                getQueryTokensHandler = ParseQuery;
                m_Entries = new string[0];
                m_WordIndexEntries = new WordIndexEntry[0];
            }

            public void Build()
            {
                CreateIndexerThread();
            }

            public bool IsReady()
            {
                return m_IndexReady;
            }

            private static void Swap<T>(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }

            public IEnumerable<EntryResult> Search(string query, int maxScore = int.MaxValue, int patternMatchLimit = 2999)
            {
                //using (new DebugTimer("File Index Search"))
                {
                    if (!m_IndexReady)
                        return Enumerable.Empty<EntryResult>();

                    var tokens = getQueryTokensHandler(query);
                    Array.Sort(tokens, SortTokensByPatternMatches);

                    var lengths = tokens.Select(p => p.Length).ToArray();
                    var patterns = tokens.Select(p => p.GetHashCode()).ToArray();

                    if (patterns.Length == 0)
                        return Enumerable.Empty<EntryResult>();

                    var wiec = new WordIndexEntryComparer();
                    var entryIndexes = new HashSet<int>();
                    lock (this)
                    {
                        var remains = GetPatternFileIndexes(patterns[0], lengths[0], maxScore, wiec, entryIndexes, patternMatchLimit).ToList();
                        patternMatchCount[patterns[0]] = remains.Count;

                        if (remains.Count == 0)
                            return Enumerable.Empty<EntryResult>();

                        //Debug.Log($"R({remains.Count>entryIndexes.Count}):" + GetDebugPatternMatchDebugString(tokens));

                        for (int i = 1; i < patterns.Length; ++i)
                        {
                            var newMatches = GetPatternFileIndexes(patterns[i], lengths[i], maxScore, wiec, entryIndexes).ToArray();
                            IntersectPatternMatches(remains, newMatches);

                            //Debug.Log($"I({entryIndexes.Count}>{newMatches.Length}>{remains.Count}):" + GetDebugPatternMatchDebugString(tokens));
                        }

                        return remains.Select(fi => new EntryResult{path = m_Entries[fi.index], index = fi.index, score = fi.score});
                    }
                }
            }

            private string GetDebugPatternMatchDebugString(string[] tokens)
            {
                return String.Join(",", tokens.Select(t =>
                {
                    patternMatchCount.TryGetValue(t.GetHashCode(), out var pmc);
                    return $"{t}({pmc})";
                }));
            }

            private int SortTokensByPatternMatches(string item1, string item2)
            {
                patternMatchCount.TryGetValue(item1.GetHashCode(), out var item1PatternMatchCount);
                patternMatchCount.TryGetValue(item2.GetHashCode(), out var item2PatternMatchCount);
                var c = item1PatternMatchCount.CompareTo(item2PatternMatchCount);
                if (c != 0) 
                    return c;
                return item1.Length.CompareTo(item2.Length) * -1;
            }

            private void IntersectPatternMatches(IList<PatternMatch> remains, PatternMatch[] newMatches)
            {
                for (int r = remains.Count - 1; r >= 0; r--)
                {
                    bool intersects = false;
                    foreach (var m in newMatches)
                    {
                        if (remains[r].index == m.index)
                        {
                            intersects = true;
                            remains[r] = new PatternMatch(m.index, Math.Min(remains[r].score, m.score));
                        }
                    }

                    if (!intersects)
                        remains.RemoveAt(r);
                }
            }

            private void LoadIndexFromDisk(string basePath)
            {
                var indexFilePath = getIndexFilePathHandler(basePath);
                if (indexFilePath == null || !File.Exists(indexFilePath))
                    return;

                //using (new DebugTimer($"Load Index (<a>{indexFilePath}</a>)"))
                {
                    var indexStream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    try
                    {
                        var indexReader = new BinaryReader(indexStream);
                        int version = indexReader.ReadInt32();
                        if (version == k_IndexFileVersion)
                        {
                            indexReader.ReadString(); // Skip
                            var elementCount = indexReader.ReadInt32();
                            var filePathEntries = new string[elementCount];
                            for (int i = 0; i < elementCount; ++i)
                                filePathEntries[i] = indexReader.ReadString();
                            elementCount = indexReader.ReadInt32();
                            var wordIndexesFromStream = new List<WordIndexEntry>(elementCount);
                            for (int i = 0; i < elementCount; ++i)
                            {
                                var key = indexReader.ReadInt32();
                                var length = indexReader.ReadInt32();
                                var fileIndex = indexReader.ReadInt32();
                                var score = indexReader.ReadInt32();
                                wordIndexesFromStream.Add(new WordIndexEntry(key, length, fileIndex, score));
                            }

                            // No need to sort the index, it is already sorted in the file stream.
                            UpdateIndexes(filePathEntries, wordIndexesFromStream);
                        }
                    }
                    finally
                    {
                        indexStream.Close();
                    }
                }

            }

            private void AbortIndexing()
            {
                if (m_IndexReady)
                    return;

                m_ThreadAborted = true;
            } 

            private void CreateIndexerThread()
            {
                m_IndexerThread = new Thread(() =>
                {
                    try
                    {
                        AssemblyReloadEvents.beforeAssemblyReload += AbortIndexing;
                        BuildWordIndexes();
                        AssemblyReloadEvents.beforeAssemblyReload -= AbortIndexing;
                    }
                    catch (ThreadAbortException)
                    {
                        m_ThreadAborted = true;
                        Thread.ResetAbort();
                    }
                });
                m_IndexerThread.Start();
            }

            private IEnumerable<WordIndexEntry> SortIndexes(List<WordIndexEntry> words)
            {
                //using (new DebugTimer($"Sort Index {roots[0].baseName}"))
                {
                    try
                    {
                        // Sort word indexes to run quick binary searches on them.
                        words.Sort(SortWordEntryComparer);
                        return words.Distinct();
                    }
                    catch
                    {
                        // This can happen while a domain reload is happening.
                        return null;
                    }
                }
            }

            private void UpdateIndexes(string[] paths, IEnumerable<WordIndexEntry> words, string saveIndexBasePath = null)
            {
                if (words == null)
                    return;

                lock (this)
                {
                    m_IndexReady = false;
                    m_Entries = paths;
                    m_WordIndexEntries = words.ToArray();
                    m_IndexReady = true;

                    if (saveIndexBasePath != null)
                    {
                        var indexFilePath = getIndexFilePathHandler(saveIndexBasePath);
                        if (String.IsNullOrEmpty(indexFilePath))
                            return;

                        //using (new DebugTimer($"Save Index (<a>{indexFilePath}</a>)"))
                        {
                            using (var indexStream = new FileStream(m_IndexTempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                BinaryWriter indexWriter = new BinaryWriter(indexStream);
                                indexWriter.Write(k_IndexFileVersion);
                                indexWriter.Write(saveIndexBasePath);

                                indexWriter.Write(m_Entries.Length);
                                foreach (var p in m_Entries)
                                    indexWriter.Write(p);
                                indexWriter.Write(m_WordIndexEntries.Length);
                                foreach (var p in m_WordIndexEntries)
                                {
                                    indexWriter.Write(p.key);
                                    indexWriter.Write(p.length);
                                    indexWriter.Write(p.fileIndex);
                                    indexWriter.Write(p.score);
                                }

                                indexStream.Close();
                            }

                            try
                            {
                                if (File.Exists(indexFilePath))
                                    File.Delete(indexFilePath);
                            }
                            catch (IOException)
                            {
                                // ignore file index persistence operation, since it is not critical and will redone later.
                            }

                            try
                            {
                                File.Move(m_IndexTempFilePath, indexFilePath);
                            }
                            catch (IOException)
                            {
                                // ignore file index persistence operation, since it is not critical and will redone later.
                            }
                        }
                    }
                }
            }

            private void BuildWordIndexes()
            {
                if (roots.Length == 0)
                    return;

                lock (this)
                    LoadIndexFromDisk(roots[0].basePath);
                
                int entryStart = 0;
                var entries = new List<string>();
                var wordIndexes = new List<WordIndexEntry>();

                //using (new DebugTimer($"Building Index for <a>{roots[0].basePath}</a>"))
                {
                    var baseScore = 0;
                    foreach (var r in roots)
                    {
                        if (m_ThreadAborted)
                            return;

                        var rootName = r.baseName;
                        var basePath = r.basePath;
                        var basePathWithSlash = basePath + "/";

                        //using (new DebugTimer($"Indexing <b>{rootName}</b> entries at <a>{basePath}</a>"))
                        {
                            if (!String.IsNullOrEmpty(rootName))
                                rootName = rootName + "/";

                            // Fetch entries to be indexed and compiled.
                            entries.AddRange(enumerateRootEntriesHandler(r));
                            BuildPartialIndex(wordIndexes, basePathWithSlash, entryStart, entries, baseScore);

                            for (int i = entryStart; i < entries.Count; ++i)
                                entries[i] = rootName + entries[i];

                            entryStart = entries.Count;
                            baseScore = 100;
                        }
                    }

                    //using (new DebugTimer($"Updating Index ({entries.Count} entries and {wordIndexes.Count} words)"))
                    {
                        UpdateIndexes(entries.ToArray(), SortIndexes(wordIndexes), roots[0].basePath);
                    }
                }
            }

            private List<WordIndexEntry> BuildPartialIndex(string basis, int entryStartIndex, IList<string> entries, int baseScore)
            {
                var wordIndexes = new List<WordIndexEntry>(entries.Count * 3);
                BuildPartialIndex(wordIndexes, basis, entryStartIndex, entries, baseScore);
                return wordIndexes;
            }

            private void BuildPartialIndex(List<WordIndexEntry> wordIndexes, string basis, int entryStartIndex, IList<string> entries, int baseScore)
            {
                for (int i = entryStartIndex; i != entries.Count; ++i)
                {
                    if (m_ThreadAborted)
                        break;

                    if (String.IsNullOrEmpty(entries[i]))
                        continue;

                    // Reformat entry to have them all uniformized.
                    if (!String.IsNullOrEmpty(basis))
                        entries[i] = entries[i].Replace('\\', '/').Replace(basis, "");

                    var path = entries[i];
                    if (skipEntryHandler(path))
                        continue;

                    var filePathComponents = getEntryComponentsHandler(path, i).ToArray();
                    //UnityEngine.Debug.LogFormat(UnityEngine.LogType.Log, UnityEngine.LogOption.NoStacktrace, null, path + " => " + String.Join(", ", filePathComponents));

                    // Build word indexes
                    for (int compIndex = 0; compIndex < filePathComponents.Length; ++compIndex)
                    {
                        var p = filePathComponents[compIndex];
                        for (int c = Math.Min(minIndexCharVariation, p.Length); c <= p.Length; ++c)
                        {
                            var ss = p.Substring(0, c);
                            wordIndexes.Add(new WordIndexEntry(ss.GetHashCode(), ss.Length, i, baseScore + compIndex));
                        }
                    }
                }
            }

            [Pure] 
            internal static int SortWordEntryComparer(WordIndexEntry item1, WordIndexEntry item2)
            {
                var c = item1.length.CompareTo(item2.length);
                if (c != 0)
                    return c;
                c = item1.key.CompareTo(item2.key);
                if (c != 0)
                    return c;
                if (item2.score == int.MaxValue)
                    return 0;
                return item1.score.CompareTo(item2.score);
            }

            private class WordIndexEntryComparer : IComparer<WordIndexEntry>
            {
                [Pure]
                public int Compare(WordIndexEntry x, WordIndexEntry y)
                {
                    return SortWordEntryComparer(x, y);
                }
            }

            private IEnumerable<PatternMatch> GetPatternFileIndexes(int key, int length, int maxScore, WordIndexEntryComparer wiec, HashSet<int> entryIndexes, int limit = int.MaxValue)
            {
                bool foundAll = entryIndexes == null || entryIndexes.Count == 0;

                // Find a match in the sorted word indexes.
                int foundIndex = Array.BinarySearch(m_WordIndexEntries, new WordIndexEntry(key, length), wiec);
                
                // Rewind to first element
                while (foundIndex > 0 && m_WordIndexEntries[foundIndex - 1].key == key && m_WordIndexEntries[foundIndex - 1].length == length)
                    foundIndex--;

                if (foundIndex < 0)
                    return Enumerable.Empty<PatternMatch>();

                var matches = new List<PatternMatch>();
                do
                {
                    bool intersects = foundAll || entryIndexes.Contains(m_WordIndexEntries[foundIndex].fileIndex);
                    if (intersects && m_WordIndexEntries[foundIndex].score < maxScore)
                    {
                        if (foundAll && entryIndexes != null)
                            entryIndexes.Add(m_WordIndexEntries[foundIndex].fileIndex);
                        matches.Add(new PatternMatch(m_WordIndexEntries[foundIndex].fileIndex, m_WordIndexEntries[foundIndex].score));

                        if (matches.Count >= limit)
                            return matches;
                    }
                    
                    // Advance to last matching element
                    foundIndex++;
                } while (foundIndex < m_WordIndexEntries.Length && m_WordIndexEntries[foundIndex].key == key && m_WordIndexEntries[foundIndex].length == length);

                return matches;
            }

            protected void UpdateIndexWithNewContent(string[] updated, string[] removed, string[] moved)
            {
                if (!m_IndexReady)
                    return;

                //using( new DebugTimer("Refreshing index with " + String.Join("\r\n\t", updated) +  $"\r\nRemoved: {String.Join("\r\n\t", removed)}" + $"\r\nMoved: {String.Join("\r\n\t", moved)}\r\n"))
                {
                    lock (this)
                    {
                        List<string> entries = null;
                        List<WordIndexEntry> words = null;

                        // Filter already known entries.
                        updated = updated.Where(u => Array.FindIndex(m_Entries, e => e == u) == -1).ToArray();

                        bool updateIndex = false;
                        if (updated.Length > 0)
                        {
                            entries = new List<string>(m_Entries);
                            words = new List<WordIndexEntry>(m_WordIndexEntries);

                            var wiec = new WordIndexEntryComparer();
                            var partialIndex = BuildPartialIndex(String.Empty, 0, updated, 0);

                            // Update entry file indexes
                            for (int i = 0; i < partialIndex.Count; ++i)
                            {
                                var pk = partialIndex[i];
                                var updatedEntry = updated[pk.fileIndex];
                                var matchedFileIndex = entries.FindIndex(e => e == updatedEntry);
                                if (matchedFileIndex == -1)
                                {
                                    entries.Add(updatedEntry);
                                    matchedFileIndex = entries.Count - 1;
                                }

                                var newWordIndex = new WordIndexEntry(pk.key, pk.length, matchedFileIndex, pk.score);
                                var insertIndex = words.BinarySearch(newWordIndex, wiec);
                                if (insertIndex > -1)
                                    words.Insert(insertIndex, newWordIndex);
                                else
                                    words.Insert(~insertIndex, newWordIndex);
                            }

                            updateIndex = true;
                        }

                        // Remove items
                        if (removed.Length > 0)
                        {
                            entries = entries ?? new List<string>(m_Entries);
                            words = words ?? new List<WordIndexEntry>(m_WordIndexEntries);

                            for (int i = 0; i < removed.Length; ++i)
                            {
                                var entryToBeRemoved = removed[i];
                                var entryIndex = entries.FindIndex(e => e == entryToBeRemoved);
                                if (entryIndex > -1)
                                    updateIndex |= words.RemoveAll(w => w.fileIndex == entryIndex) > 0;
                            }
                        }

                        if (updateIndex)
                        {
                            UpdateIndexes(entries.ToArray(), SortIndexes(words));
                        }
                    }
                }
            }

            private string[] ParseQuery(string query)
            {
                return query.Trim().ToLowerInvariant()
                            .Split(entrySeparators)
                            .Select(t => t.Substring(0, Math.Min(t.Length, maxIndexCharVariation)))
                            .Where(t => t.Length > 0)
                            .OrderBy(t => -t.Length).ToArray();
            }
        }
    }
}
