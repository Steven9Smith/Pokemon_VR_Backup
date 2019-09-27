using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Unity.QuickSearch
{

    [DebuggerDisplay("{searchQuery}")]
    public class SearchContext
    {
        // Raw search text (i.e. what is in the search text box)
        public string searchText;
        // Processed search query: filterId were removed.
        public string searchQuery;
        // Search query tokenized by words.
        public string[] tokenizedSearchQuery;
        // Search query tokenized by words all in lower case.
        public string[] tokenizedSearchQueryLower;
        // All tokens containing a colon (':')
        public string[] textFilters;
        // All sub categories related to this provider and their enabled state.
        public List<SearchFilter.Entry> categories;
        // Mark the number of item found after running the search.
        public int totalItemCount;
        // Editor window that initiated the search
        public EditorWindow focusedWindow;
        // Indicates if the search should return results as many as possible.
        public bool wantsMore;

        public string actionQueryId;
        public bool isActionQuery;

        /// <summary>
        /// Search view holding and presenting the search results.
        /// </summary>
        internal ISearchView searchView;

        static public readonly SearchContext Empty = new SearchContext {searchText = String.Empty, searchQuery = String.Empty};
    }
}