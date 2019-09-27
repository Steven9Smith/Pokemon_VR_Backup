using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Unity.QuickSearch
{
    /// <summary>
    /// An async search session tracks all incoming items found by search provider that weren't returned right away after the search was initiated.
    /// </summary>
    public class AsyncSearchSession
    {
        /// <summary>
        /// This event is used to receive any async search result.
        /// </summary>
        /// <remarks>It is usually used by a search view to append additional search results to a UI list.</remarks>
        public static event Action<IEnumerable<SearchItem>> asyncItemReceived;

        private const long k_MaxTimePerUpdate = 10; // milliseconds

        private IEnumerator<SearchItem> m_ItemsIterator;
        private bool m_IsRunning = false;
        private long m_MaxFetchTimePerProviderMs;

        private static int s_RunningSessions = 0;

        /// <summary>
        /// Checks if there is any active async search sessions.
        /// </summary>
        public static bool SearchInProgress => s_RunningSessions > 0;

        /// <summary>
        /// Called when the system is ready to process any new async results.
        /// </summary>
        public void OnUpdate()
        {
            var newItems = new List<SearchItem>();
            var atEnd = !FetchSome(newItems, m_MaxFetchTimePerProviderMs);

            if (newItems.Count > 0)
                asyncItemReceived?.Invoke(newItems);

            if (atEnd)
            {
                Stop();
            }
        }

        /// <summary>
        /// Hard reset an async search session.
        /// </summary>
        /// <param name="itemEnumerator">The enumerator that will yield new search results.</param>
        /// <param name="maxFetchTimePerProviderMs">The amount of time allowed to yield new results.</param>
        /// <remarks>Normally async search sessions are re-used per search provider.</remarks>
        public void Reset(IEnumerator<SearchItem> itemEnumerator, long maxFetchTimePerProviderMs = k_MaxTimePerUpdate)
        {
            // Remove and add the event handler in case it was already removed.
            Stop();
            m_IsRunning = true;
            m_MaxFetchTimePerProviderMs = maxFetchTimePerProviderMs;
            ++s_RunningSessions;
            m_ItemsIterator = itemEnumerator;
            EditorApplication.update += OnUpdate;
        }

        /// <summary>
        /// Stop the async search session and discard any new search results.
        /// </summary>
        public void Stop()
        {
            if (m_IsRunning)
                --s_RunningSessions;
            m_IsRunning = false;
            EditorApplication.update -= OnUpdate;
            m_ItemsIterator = null;
        }

        /// <summary>
        /// Request to fetch new async search results.
        /// </summary>
        /// <param name="items">The list of items to append new results to.</param>
        /// <param name="quantity">The maximum amount of items to be added to @items</param>
        /// <param name="doNotCountNull">Ignore all yield return null results.</param>
        /// <returns>Returns true if there is still some results to fetch later or false if we've fetched everything remaining.</returns>
        public bool FetchSome(List<SearchItem> items, int quantity, bool doNotCountNull)
        {
            if (m_ItemsIterator == null)
                return false;

            var atEnd = false;
            for (var i = 0; i < quantity && !atEnd; ++i)
            {
                atEnd = !m_ItemsIterator.MoveNext();
                if (!atEnd)
                {
                    if (m_ItemsIterator.Current == null)
                    {
                        if (doNotCountNull)
                            --i;
                        continue;
                    }
                    items.Add(m_ItemsIterator.Current);
                }
            }

            return !atEnd;
        }

        /// <summary>
        /// Request to fetch new async search results.
        /// </summary>
        /// <param name="items">The list of items to append new results to.</param>
        /// <param name="quantity">The maximum amount of items to add to @items</param>
        /// <param name="doNotCountNull">Ignore all yield return null results.</param>
        /// <param name="maxFetchTimeMs">The amount of time allowed to yield new results.</param>
        /// <returns>Returns true if there is still some results to fetch later or false if we've fetched everything remaining.</returns>
        public bool FetchSome(List<SearchItem> items, int quantity, bool doNotCountNull, long maxFetchTimeMs)
        {
            if (m_ItemsIterator == null)
                return false;

            var atEnd = false;
            var timeToFetch = Stopwatch.StartNew();
            for (var i = 0; i < quantity && !atEnd && timeToFetch.ElapsedMilliseconds < maxFetchTimeMs; ++i)
            {
                atEnd = !m_ItemsIterator.MoveNext();
                if (!atEnd)
                {
                    if (m_ItemsIterator.Current == null)
                    {
                        if (doNotCountNull)
                            --i;
                        continue;
                    }
                    items.Add(m_ItemsIterator.Current);
                }
            }

            return !atEnd;
        }

        /// <summary>
        /// Request to fetch new async search results.
        /// </summary>
        /// <param name="items">The list of items to append new results to.</param>
        /// <param name="maxFetchTimeMs">The amount of time allowed to yield new results.</param>
        /// <returns>Returns true if there is still some results to fetch later or false if we've fetched everything remaining.</returns>
        public bool FetchSome(List<SearchItem> items, long maxFetchTimeMs)
        {
            if (m_ItemsIterator == null)
                return false;

            var atEnd = false;
            var timeToFetch = Stopwatch.StartNew();
            while (!atEnd && timeToFetch.ElapsedMilliseconds < maxFetchTimeMs)
            {
                atEnd = !m_ItemsIterator.MoveNext();
                if (!atEnd && m_ItemsIterator.Current != null)
                    items.Add(m_ItemsIterator.Current);
            }

            return !atEnd;
        }
    }
}