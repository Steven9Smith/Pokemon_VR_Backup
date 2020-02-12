//#define QUICKSEARCH_DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

#if QUICKSEARCH_DEBUG
using UnityEngine.Profiling;
#endif

namespace Unity.QuickSearch
{
    using ItemsById = SortedDictionary<string, SearchItem>;
    using ItemsByScore = SortedDictionary<int, SortedDictionary<string, SearchItem>>;
    using ItemsByProvider = SortedDictionary<int, SortedDictionary<int, SortedDictionary<string, SearchItem>>>;
    internal class AutoSortedSearchItemsList : IEnumerable<SearchItem>
    {
        private class IdComparer : Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                return String.Compare(x, y, StringComparison.Ordinal);
            }
        }

        private ItemsByProvider m_Data = new ItemsByProvider();
        private Dictionary<string, Tuple<int, int>> m_LUT = new Dictionary<string, Tuple<int, int>>();

        private bool m_TemporaryUnordered = false;
        private List<SearchItem> m_UnorderedItems = new List<SearchItem>();

        public int Count { get; private set; }

        public SearchItem this[int index] => this.ElementAt(index);

        public AutoSortedSearchItemsList(IEnumerable<SearchItem> items)
        {
            FromEnumerable(items);
        }

        public static implicit operator AutoSortedSearchItemsList(List<SearchItem> items)
        {
            return new AutoSortedSearchItemsList(items);
        }

        public void FromEnumerable(IEnumerable<SearchItem> items)
        {
            Clear();
            AddItems(items);
        }

        public void AddItems(IEnumerable<SearchItem> items)
        {
            foreach (var item in items)
            {
                bool shouldAdd = true;
                if (m_LUT.ContainsKey(item.id))
                {
                    var alreadyContainedValues = m_LUT[item.id];
                    if (item.provider.priority >= alreadyContainedValues.Item1 &&
                        item.score >= alreadyContainedValues.Item2)
                        shouldAdd = false;

                    if (shouldAdd)
                    {
                        m_Data[alreadyContainedValues.Item1][alreadyContainedValues.Item2].Remove(item.id);
                        m_LUT.Remove(item.id);
                        --Count;
                    }
                }

                if (!shouldAdd)
                    continue;

                if (!m_Data.TryGetValue(item.provider.priority, out var itemsByScore))
                {
                    itemsByScore = new ItemsByScore();
                    m_Data.Add(item.provider.priority, itemsByScore);
                }

                if (!itemsByScore.TryGetValue(item.score, out var itemsById))
                {
                    itemsById = new ItemsById(new IdComparer());
                    itemsByScore.Add(item.score, itemsById);
                }

                itemsById.Add(item.id, item);
                m_LUT.Add(item.id, new Tuple<int, int>(item.provider.priority, item.score));
                ++Count;
            }
        }

        public void Clear()
        {
            m_Data.Clear();
            m_LUT.Clear();
            Count = 0;
            m_TemporaryUnordered = false;
            m_UnorderedItems.Clear();
        }

        public IEnumerator<SearchItem> GetEnumerator()
        {
            if (m_TemporaryUnordered)
            {

                foreach (var item in m_UnorderedItems)
                {
                    yield return item;
                }
            }

            foreach (var itemsByPriority in m_Data)
            {
                foreach (var itemsByScore in itemsByPriority.Value)
                {
                    foreach (var itemsById in itemsByScore.Value)
                    {
                        yield return itemsById.Value;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<SearchItem> GetRange(int skipCount, int count)
        {
            int skipped = 0;
            int counted = 0;
            foreach (var item in this)
            {
                if (skipped < skipCount)
                {
                    ++skipped;
                    continue;
                }

                if (counted >= count)
                    yield break;

                yield return item;
                ++counted;
            }
        }

        public void InsertRange(int index, IEnumerable<SearchItem> items)
        {
            if (!m_TemporaryUnordered)
            {
                m_TemporaryUnordered = true;
                m_UnorderedItems = this.ToList();
            }

            var tempList = items.ToList();
            m_UnorderedItems.InsertRange(index, tempList);
            Count += tempList.Count;
        }
    }

    internal class QuickSearchTool : EditorWindow, ISearchView
    {
        public static string packageName = "com.unity.quicksearch";
        public static string packageFolderName = $"Packages/{packageName}";

        private static EditorWindow s_FocusedWindow;
        private static bool isDeveloperMode = Utils.IsDeveloperMode();

        private const int k_ResetSelectionIndex = -1;
        private const float k_SearchInProgressButtonRotationIncrement = 15.0f;
        private const string k_QuickSearchBoxName = "QuickSearchBox";
        private const string s_Helpme = "Search {0}!\r\n\r\n" +
            "- <b>Alt + Up/Down Arrow</b> \u2192 Search history\r\n" +
            "- <b>Alt + Left</b> \u2192 Filter\r\n" +
            "- <b>Alt + Right</b> \u2192 Actions menu\r\n" +
            "- <b>Enter</b> \u2192 Default action\r\n" +
            "- <b>Alt + Enter</b> \u2192 Secondary action\r\n" +
            "- Drag items around\r\n" +
            "- Type <b>?</b> to get help\r\n";

        [SerializeField] private Vector2 m_ScrollPosition;
        [SerializeField] public EditorWindow lastFocusedWindow;
        [SerializeField] private int m_SelectedIndex = k_ResetSelectionIndex;
        [SerializeField] private string m_SearchTopic = "anything";

        private event Action nextFrame;
        internal bool m_SendAnalyticsEvent;
        private SearchContext m_Context;
        private AutoSortedSearchItemsList m_FilteredItems;
        private bool m_FocusSelectedItem = false;
        private Rect m_ScrollViewOffset;
        private bool m_SearchBoxFocus;
        private int m_SearchBoxControlID = -1;
        private double m_ClickTime = 0;
        private bool m_CursorBlinking;
        private bool m_IsRepaintAfterTimeRequested = false;
        private double m_RequestRepaintAfterTime = 0;
        private double m_NextBlinkTime = 0;
        private bool m_PrepareDrag;
        private string m_CycledSearch;
        private bool m_ShowFilterWindow = false;
        private SearchAnalytics.SearchEvent m_CurrentSearchEvent;
        private double m_DebounceTime = 0.0;
        private float m_Height = 0;
        private GUIContent m_StatusLabelContent = new GUIContent();
        private int m_DelayedCurrentSelection = -1;
        private float m_CurrentSearchInProgressButtonRotation = 0.0f;

        private static class Styles
        {
            static Styles()
            {
                if (!isDarkTheme)
                {
                    selectedItemLabel.normal.textColor = Color.white;
                    selectedItemDescription.normal.textColor = Color.white;
                }
            }

            private const int itemRowPadding = 4;
            public const float actionButtonSize = 24f;
            public const float itemPreviewSize = 32f;
            public const float itemRowSpacing = 30.0f;
            private const int actionButtonMargin = (int)((itemRowHeight - actionButtonSize) / 2f);
            public const float itemRowHeight = itemPreviewSize + itemRowPadding * 2f;
            public const float statusOffset = 20;

            private static bool isDarkTheme => EditorGUIUtility.isProSkin;

            private static readonly RectOffset marginNone = new RectOffset(0, 0, 0, 0);
            private static readonly RectOffset paddingNone = new RectOffset(0, 0, 0, 0);
            private static readonly RectOffset defaultPadding = new RectOffset(itemRowPadding, itemRowPadding, itemRowPadding, itemRowPadding);

            private static readonly Color darkColor1 = new Color(61 / 255f, 61 / 255f, 61 / 255f);
            private static readonly Color darkColor2 = new Color(71 / 255f, 106 / 255f, 155 / 255f);
            private static readonly Color darkColor3 = new Color(68 / 255f, 68 / 255f, 71 / 255f);
            private static readonly Color darkColor4 = new Color(111 / 255f, 111 / 255f, 111 / 255f);
            private static readonly Color darkColor5 = new Color(71 / 255f, 71 / 255f, 71 / 255f);
            private static readonly Color darkColor6 = new Color(63 / 255f, 63 / 255f, 63 / 255f);
            private static readonly Color darkColor7 = new Color(71 / 255f, 71 / 255f, 71 / 255f);

            private static readonly Color lightColor1 = new Color(171 / 255f, 171 / 255f, 171 / 255f);
            private static readonly Color lightColor2 = new Color(71 / 255f, 106 / 255f, 155 / 255f);
            private static readonly Color lightColor3 = new Color(168 / 255f, 168 / 255f, 171 / 255f);
            private static readonly Color lightColor4 = new Color(111 / 255f, 111 / 255f, 111 / 255f);
            private static readonly Color lightColor5 = new Color(181 / 255f, 181 / 255f, 181 / 255f);
            private static readonly Color lightColor6 = new Color(214 / 255f, 214 / 255f, 214 / 255f);
            private static readonly Color lightColor7 = new Color(230 / 255f, 230 / 255f, 230 / 255f);

            public static readonly string highlightedTextColorFormat = isDarkTheme ? "<color=#F6B93F>{0}</color>" : "<b>{0}</b>";

            private static readonly Color textAutoCompleteBgColorDark = new Color(37 / 255.0f, 37 / 255.0f, 38 / 255.0f);
            private static readonly Color textAutoCompleteBgColorLight = new Color(165 / 255.0f, 165 / 255.0f, 165 / 255.0f);
            public static readonly Color textAutoCompleteBgColor = isDarkTheme ? textAutoCompleteBgColorDark : textAutoCompleteBgColorLight;
            private static readonly Color textAutoCompleteSelectedColorDark = new Color(7 / 255.0f, 54 / 255.0f, 85 / 255.0f);
            private static readonly Color textAutoCompleteSelectedColorLight = new Color(58 / 255.0f, 114 / 255.0f, 176 / 255.0f);
            public static readonly Color textAutoCompleteSelectedColor = isDarkTheme ? textAutoCompleteSelectedColorDark : textAutoCompleteSelectedColorLight;

            #if !UNITY_2019_3_OR_NEWER
            private static readonly Color darkSelectedRowColor = new Color(61 / 255f, 96 / 255f, 145 / 255f);
            private static readonly Color lightSelectedRowColor = new Color(61 / 255f, 128 / 255f, 223 / 255f);
            private static readonly Texture2D alternateRowBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor1 : lightColor1);
            private static readonly Texture2D selectedRowBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkSelectedRowColor : lightSelectedRowColor);
            private static readonly Texture2D selectedHoveredRowBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor2 : lightColor2);
            private static readonly Texture2D hoveredRowBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor3 : lightColor3);
            #endif

            private static readonly Texture2D buttonPressedBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor4 : lightColor4);
            private static readonly Texture2D buttonHoveredBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor5 : lightColor5);

            private static readonly Texture2D searchFieldBg = GenerateSolidColorTexture(isDarkTheme ? darkColor6 : lightColor6);
            private static readonly Texture2D searchFieldFocusBg = GenerateSolidColorTexture(isDarkTheme ? darkColor7 : lightColor7);

            public static readonly GUIStyle panelBorder = new GUIStyle("grey_border")
            {
                name = "quick-search-border",
                padding = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(0, 0, 0, 0)
            };
            public static readonly GUIContent filterButtonContent = new GUIContent("", Icons.filter, "Open search filter window (Alt + Left)");
            public static readonly GUIContent moreActionsContent = new GUIContent("", Icons.more, "Open actions menu (Alt + Right)");

            public static readonly GUIStyle itemBackground1 = new GUIStyle
            {
                name = "quick-search-item-background1",
                fixedHeight = itemRowHeight,

                margin = marginNone,
                padding = defaultPadding,

                #if !UNITY_2019_3_OR_NEWER
                hover = new GUIStyleState { background = hoveredRowBackgroundImage, scaledBackgrounds = new[] { hoveredRowBackgroundImage } }
                #endif
            };

            public static readonly GUIStyle itemBackground2 = new GUIStyle(itemBackground1)
            {
                name = "quick-search-item-background2",

                #if !UNITY_2019_3_OR_NEWER
                normal = new GUIStyleState { background = alternateRowBackgroundImage, scaledBackgrounds = new[] { alternateRowBackgroundImage } }
                #endif
            };

            public static readonly GUIStyle selectedItemBackground = new GUIStyle(itemBackground1)
            {
                name = "quick-search-item-selected-background",

                #if !UNITY_2019_3_OR_NEWER
                normal = new GUIStyleState { background = selectedRowBackgroundImage, scaledBackgrounds = new[] { selectedRowBackgroundImage } },
                hover = new GUIStyleState { background = selectedHoveredRowBackgroundImage, scaledBackgrounds = new[] { selectedHoveredRowBackgroundImage } }
                #endif
            };

            public static readonly GUIStyle preview = new GUIStyle
            {
                name = "quick-search-item-preview",
                fixedWidth = itemPreviewSize,
                fixedHeight = itemPreviewSize,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly,
                margin = new RectOffset(2, 2, 2, 2),
                padding = paddingNone
            };

            public static readonly GUIStyle itemLabel = new GUIStyle(EditorStyles.label)
            {
                name = "quick-search-item-label",
                richText = true,
                wordWrap = false,
                margin = new RectOffset(4, 4, 6, 2),
                padding = paddingNone
            };

            public static readonly GUIStyle selectedItemLabel = new GUIStyle(itemLabel)
            {
                name = "quick-search-item-selected-label",

                margin = new RectOffset(4, 4, 6, 2),
                padding = paddingNone
            };

            public static readonly GUIStyle noResult = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                name = "quick-search-no-result",
                fontSize = 20,
                fixedHeight = 0,
                fixedWidth = 0,
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                margin = marginNone,
                padding = paddingNone
            };

            public static readonly GUIStyle itemDescription = new GUIStyle(EditorStyles.label)
            {
                name = "quick-search-item-description",
                richText = true,
                wordWrap = false,
                margin = new RectOffset(4, 4, 1, 4),
                padding = paddingNone,

                fontSize = Math.Max(9, itemLabel.fontSize - 2),
                fontStyle = FontStyle.Italic
            };

            public static readonly GUIStyle statusLabel = new GUIStyle(itemDescription)
            {
                name = "quick-search-status-label",
                margin = new RectOffset(4, 4, 2, 2)
            };

            public static readonly GUIStyle selectedItemDescription = new GUIStyle(itemDescription)
            {
                name = "quick-search-item-selected-description"
            };

            public static readonly GUIStyle actionButton = new GUIStyle("IconButton")
            {
                name = "quick-search-action-button",

                fixedWidth = actionButtonSize,
                fixedHeight = actionButtonSize,

                imagePosition = ImagePosition.ImageOnly,

                margin = new RectOffset(4, 4, actionButtonMargin, actionButtonMargin),
                padding = paddingNone,

                active = new GUIStyleState { background = buttonPressedBackgroundImage, scaledBackgrounds = new[] { buttonPressedBackgroundImage } },
                hover = new GUIStyleState { background = buttonHoveredBackgroundImage, scaledBackgrounds = new[] { buttonHoveredBackgroundImage } }
            };

            private const float k_ToolbarHeight = 40.0f;

            private static readonly GUIStyleState clear = new GUIStyleState()
            {
                background = null,
                scaledBackgrounds = new Texture2D[] { null },
                textColor = isDarkTheme ? new Color (210 / 255f, 210 / 255f, 210 / 255f) : Color.black
            };

            private static readonly GUIStyleState searchFieldBgNormal = new GUIStyleState() { background = searchFieldBg, scaledBackgrounds = new Texture2D[] { null } };
            private static readonly GUIStyleState searchFieldBgFocus = new GUIStyleState() { background = searchFieldFocusBg, scaledBackgrounds = new Texture2D[] { null } };

            public static readonly GUIStyle toolbar = new GUIStyle("Toolbar")
            {
                name = "quick-search-bar",
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                fixedHeight = k_ToolbarHeight,

                normal = searchFieldBgNormal,
                focused = searchFieldBgFocus, hover = searchFieldBgFocus, active = searchFieldBgFocus,
                onNormal = clear, onHover = searchFieldBgFocus, onFocused = searchFieldBgFocus, onActive = searchFieldBgFocus,
            };

            public static readonly GUIStyle searchField = new GUIStyle("ToolbarSeachTextFieldPopup")
            {
                name = "quick-search-search-field",
                fontSize = 28,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0,
                normal = clear,
                focused = clear, hover = clear, active = clear,
                onNormal = clear, onHover = clear, onFocused = clear, onActive = clear,
            };

            public static readonly GUIStyle placeholderTextStyle = new GUIStyle(searchField)
            {
                fontSize = 20,
                fontStyle = FontStyle.Italic,
                padding = new RectOffset(6, 0, 0, 0)
            };

            public static readonly GUIStyle searchFieldClear = new GUIStyle()
            {
                name = "quick-search-search-field-clear",
                fixedHeight = 0,
                fixedWidth = 0,
                margin = new RectOffset(0, 5, 8, 0),
                padding = new RectOffset(0, 0, 0, 0),
                normal = clear,
                focused = clear, hover = clear, active = clear,
                onNormal = clear, onHover = clear, onFocused = clear, onActive = clear
            };

            public static readonly GUIStyle filterButton = new GUIStyle(EditorStyles.whiteLargeLabel)
            {
                name = "quick-search-filter-button",
                margin = new RectOffset(-4, 0, 0, 0),
                padding = new RectOffset(0, 0, 1, 0),
                normal = clear,
                focused = clear, hover = clear, active = clear,
                onNormal = clear, onHover = clear, onFocused = clear, onActive = clear
            };

            public static readonly GUIContent prefButtonContent = new GUIContent(Icons.settings, "Open quick search preferences...");
            public static readonly GUIStyle prefButton = new GUIStyle("IconButton")
            {
                fixedWidth = 16, fixedHeight = 16,
                margin = new RectOffset(0, 0, 0, 2),
                padding = new RectOffset(0, 0, 0, 0)
            };

            public static readonly GUIContent searchInProgressContent = new GUIContent(Icons.loading, "Open quick search preferences...");
            public static readonly GUIStyle searchInProgressButton = new GUIStyle("IconButton")
            {
                fixedWidth = 16,
                fixedHeight = 16,
                margin = new RectOffset(0, 0, 0, 2),
                padding = new RectOffset(0, 0, 0, 0)
            };
            public static readonly GUILayoutOption[] searchInProgressLayoutOptions = new[] { GUILayout.MaxWidth(searchInProgressButton.fixedWidth) };

            private static Texture2D GenerateSolidColorTexture(Color fillColor)
            {
                Texture2D texture = new Texture2D(1, 1);
                var fillColorArray = texture.GetPixels();

                for (var i = 0; i < fillColorArray.Length; ++i)
                    fillColorArray[i] = fillColor;

                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.SetPixels(fillColorArray);
                texture.Apply();

                return texture;
            }
        }

        internal static Rect ContextualActionPosition { get; private set; }

        internal SearchContext Context => m_Context;
        internal IEnumerable<SearchItem> Results => m_FilteredItems;

        [UsedImplicitly]
        internal void OnEnable()
        {
            m_CurrentSearchEvent = new SearchAnalytics.SearchEvent();
            m_Context = new SearchContext { searchText = String.Empty, focusedWindow = lastFocusedWindow, searchView = this };
            SearchService.Enable(m_Context);
            m_Context.searchText = SearchService.LastSearch;
            m_SearchBoxFocus = true;
            lastFocusedWindow = s_FocusedWindow;
            UpdateWindowTitle();

            Refresh();

            AsyncSearchSession.asyncItemReceived += OnAsyncItemsReceived;
        }

        [UsedImplicitly]
        internal void OnDisable()
        {
            EditorApplication.delayCall -= DebouncedRefresh;
            s_FocusedWindow = null;

            if (!isDeveloperMode && !SearchSettings.useDockableWindow)
                SendSearchEvent(null); // Track canceled searches

            SearchService.Disable(m_Context);
            AsyncSearchSession.asyncItemReceived -= OnAsyncItemsReceived;
        }

        public void SetSearchText(string searchText)
        {
            if (searchText == null)
                return;

            m_Context.searchText = searchText;
            Refresh();

            var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), m_SearchBoxControlID);
            nextFrame += () => te.MoveLineEnd();
        }

        public void PopFilterWindow()
        {
            nextFrame += () => m_ShowFilterWindow = true;
        }

        private void OnAsyncItemsReceived(IEnumerable<SearchItem> items)
        {
            if (m_SelectedIndex == -1)
            {
                //using (new DebugTimer("SortItemsAsync"))
                {
                    m_FilteredItems.AddItems(items);
                }
            }
            else
            {
                m_FilteredItems.InsertRange(m_SelectedIndex + 1, items);
            }

            Repaint();
        }

        private void SendSearchEvent(SearchItem item, SearchAction action = null)
        {
            if (item != null)
                m_CurrentSearchEvent.Success(item, action);

            if (m_CurrentSearchEvent.success || m_CurrentSearchEvent.elapsedTimeMs > 7000)
            {
                m_CurrentSearchEvent.Done();
                if (item != null)
                    m_CurrentSearchEvent.searchText = $"{m_Context.searchText} => {item.id}";
                else
                    m_CurrentSearchEvent.searchText = m_Context.searchText;
                if (m_SendAnalyticsEvent)
                    SearchAnalytics.SendSearchEvent(m_CurrentSearchEvent);
            }

            // Prepare next search event
            m_CurrentSearchEvent = new SearchAnalytics.SearchEvent();
        }

        private void UpdateWindowTitle()
        {
            titleContent.image = Icons.quicksearch;
            if (m_FilteredItems == null || m_FilteredItems.Count == 0)
                titleContent.text = $"Search {m_SearchTopic}!";
            else
            {
                var itemStr = m_FilteredItems.Count <= 1 ? "item" : "items";
                titleContent.text = $"Found {m_FilteredItems.Count} {itemStr}!";
            }
        }

        private void DrawAutoCompletion(Rect rect)
        {
            if (m_DiscardAutoComplete || m_SearchBoxControlID <= 0)
                return;

            var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), m_SearchBoxControlID);
            var cursorPosition = te.cursorIndex;
            if (cursorPosition == 0)
                return;

            var searchText = m_Context.searchText;
            var lastTokenStartPos = searchText.LastIndexOf(' ', Math.Max(0, te.cursorIndex - 1));
            var lastToken = lastTokenStartPos == -1 ? searchText : searchText.Substring(lastTokenStartPos + 1);
            var keywords = SearchService.GetKeywords(m_Context, lastToken)
                .Where(k => !k.Equals(lastToken, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (keywords.Length > 0)
            {
                const int maxAutoCompleteCount = 16;
                m_AutoCompleteMaxIndex = Math.Min(keywords.Length, maxAutoCompleteCount);
                if (!m_AutoCompleting)
                    m_AutoCompleteIndex = 0;

                if (Event.current.type == EventType.Repaint)
                {
                    var content = new GUIContent(m_Context.searchText.Substring(0, m_Context.searchText.Length - lastToken.Length));
                    var offset = Styles.searchField.CalcSize(content).x;
                    m_AutoCompleteRect = rect;
                    m_AutoCompleteRect.x += offset;
                    m_AutoCompleteRect.y = rect.yMax;
                    m_AutoCompleteRect.width = 250;
                    m_AutoCompleteRect.x = Math.Min(position.width - m_AutoCompleteRect.width - 25, m_AutoCompleteRect.x);
                }

                var autoFill = TextFieldAutoComplete(m_AutoCompleteRect, lastToken, keywords, maxAutoCompleteCount, 0.4f);
                if (autoFill == null)
                {
                    // No more results
                    m_AutoCompleting = false;
                    m_AutoCompleteIndex = -1;
                }
                else if (autoFill != lastToken)
                {
                    m_AutoCompleting = false;
                    m_DiscardAutoComplete = true;
                    var regex = new Regex(Regex.Escape(lastToken), RegexOptions.IgnoreCase);
                    autoFill = regex.Replace(autoFill, "");
                    m_Context.searchText = m_Context.searchText.Insert(cursorPosition, autoFill);
                    Refresh();
                    nextFrame += () => te.MoveToStartOfNextWord();
                }
                else
                    m_AutoCompleting = true;
            }
            else
            {
                m_AutoCompleting = false;
                m_AutoCompleteIndex = -1;
            }
        }

        internal void DrawStatusBar()
        {
            var msg = "";
            if (m_Context.isActionQuery)
                msg = "Action for ";

            IEnumerable<SearchProvider> providerList = SearchService.Filter.filteredProviders;
            if (SearchService.OverrideFilter.filteredProviders.Count > 0)
            {
                if (SearchService.OverrideFilter.filteredProviders.All(p => p.isExplicitProvider))
                {
                    if (msg.Length == 0)
                        msg = "Activate ";
                }
                else
                {
                    if (msg.Length == 0)
                        msg = "Searching only ";
                }

                providerList = SearchService.OverrideFilter.filteredProviders;
            }
            else
            {
                if (msg.Length == 0)
                    msg = "Searching ";
            }

            msg += Utils.FormatProviderList(providerList);

            if (m_FilteredItems != null && m_FilteredItems.Count > 0)
                msg += $" and found <b>{m_FilteredItems.Count}</b> results";

            m_StatusLabelContent.text = msg;
            m_StatusLabelContent.tooltip = Utils.FormatProviderList(providerList, true);

            GUILayout.BeginHorizontal();
            GUILayout.Label(m_StatusLabelContent, Styles.statusLabel, GUILayout.MaxWidth(position.width - 100));
            GUILayout.FlexibleSpace();

            GUILayout.Label(SearchAnalytics.Version, Styles.statusLabel);
            if (AsyncSearchSession.SearchInProgress)
            {
                var searchInProgressRect = EditorGUILayout.GetControlRect(false, Styles.searchInProgressButton.fixedHeight, Styles.searchInProgressButton, Styles.searchInProgressLayoutOptions);
                var pivot = new Vector2(searchInProgressRect.xMin + searchInProgressRect.width * 0.5f, searchInProgressRect.yMin + searchInProgressRect.height * 0.5f);
                var oldMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(m_CurrentSearchInProgressButtonRotation, pivot);

                GUI.DrawTexture(searchInProgressRect, Styles.searchInProgressContent.image);
                GUI.matrix = oldMatrix;
                m_CurrentSearchInProgressButtonRotation += k_SearchInProgressButtonRotationIncrement;

                if (Event.current.type == EventType.MouseDown && searchInProgressRect.Contains(Event.current.mousePosition))
                {
                    SettingsService.OpenUserPreferences(SearchSettings.settingsPreferencesKey);
                }
            }
            else
            {
                m_CurrentSearchInProgressButtonRotation = 0.0f;
                if (GUILayout.Button(Styles.prefButtonContent, Styles.prefButton))
                    SettingsService.OpenUserPreferences(SearchSettings.settingsPreferencesKey);
            }

            GUILayout.EndHorizontal();
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            #if QUICKSEARCH_DEBUG
            Profiler.BeginSample("QuickSearch");
            using (new DebugTimer("QuickSearch.OnGUI." + Event.current.type))
            #endif
            {
                if (Event.current.type == EventType.Repaint)
                {
                    nextFrame?.Invoke();
                    nextFrame = null;
                }

                if (m_Height != position.height)
                    OnResize();

                HandleKeyboardNavigation(m_Context);

                if (!SearchSettings.useDockableWindow)
                    EditorGUILayout.BeginVertical(Styles.panelBorder);
                {
                    var rect = DrawToolbar(m_Context);
                    DrawItems(m_Context);
                    DrawAutoCompletion(rect);
                    DrawStatusBar();
                }
                if (!SearchSettings.useDockableWindow)
                    EditorGUILayout.EndVertical();

                UpdateFocusControlState();
            }

            #if QUICKSEARCH_DEBUG
            Profiler.EndSample();
            #endif
        }

        [UsedImplicitly]
        internal void OnFocus()
        {
            if (SearchSettings.useDockableWindow)
                m_SearchBoxFocus = true;
        }

        internal void OnResize()
        {
            if (m_Height > 0 && m_ScrollPosition.y > 0)
                m_ScrollPosition.y -= position.height - m_Height;
            m_Height = position.height;
        }

        public void Refresh()
        {
            m_FilteredItems = SearchService.GetItems(m_Context);
            SetSelection(k_ResetSelectionIndex);
            UpdateWindowTitle();
            Repaint();
        }

        private int SetSelection(int selection)
        {
            var previousSelection = m_SelectedIndex;
            m_SelectedIndex = Math.Max(-1, Math.Min(selection, m_FilteredItems.Count - 1));
            if (m_SelectedIndex == k_ResetSelectionIndex)
            {
                m_ScrollPosition.y = 0;
                m_DiscardAutoComplete = false;
            }
            if (previousSelection != m_SelectedIndex)
                RaiseSelectionChanged(m_SelectedIndex);
            return m_SelectedIndex;
        }

        private void RaiseSelectionChanged(int currentSelection)
        {
            if (currentSelection == -1)
                return;

            TrackSelection(currentSelection);
        }

        private void DelayTrackSelection()
        {
            if (m_FilteredItems == null || m_FilteredItems.Count == 0)
                return;

            if (m_DelayedCurrentSelection < 0 || m_DelayedCurrentSelection >= m_FilteredItems.Count)
                return;

            var selectedItem = m_FilteredItems[m_DelayedCurrentSelection];
            selectedItem.provider?.trackSelection?.Invoke(selectedItem, m_Context);

            m_DelayedCurrentSelection = -1;
        }

        private void TrackSelection(int currentSelection)
        {
            if (!SearchSettings.trackSelection)
                return;

            m_DelayedCurrentSelection = currentSelection;
            EditorApplication.delayCall -= () => DelayTrackSelection();
            EditorApplication.delayCall += () => DelayTrackSelection();
        }

        private void UpdateFocusControlState()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (m_SearchBoxFocus)
            {
                EditorGUI.FocusTextInControl(k_QuickSearchBoxName);
                m_SearchBoxFocus = false;
            }
        }

        private int GetDisplayItemCount()
        {
            if (m_FilteredItems == null)
                return 0;
            var itemCount = m_FilteredItems.Count;
            var availableHeight = position.height - m_ScrollViewOffset.yMax;
            return Math.Max(0, Math.Min(itemCount, (int)(availableHeight / Styles.itemRowHeight) + 2));
        }

        private void HandleKeyboardNavigation(SearchContext context)
        {
            var evt = Event.current;
            if (evt.type == EventType.KeyDown)
            {
                var prev = m_SelectedIndex;
                if (evt.keyCode == KeyCode.DownArrow)
                {
                    if (m_AutoCompleting)
                    {
                        m_AutoCompleteIndex = SearchService.Wrap(m_AutoCompleteIndex + 1, m_AutoCompleteMaxIndex);
                        Event.current.Use();
                    }
                    else
                    {
                        if (m_SelectedIndex == -1 && evt.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            m_CurrentSearchEvent.useHistoryShortcut = true;
                            m_CycledSearch = SearchService.CyclePreviousSearch(-1);
                            GUI.FocusControl(null);
                        }
                        else
                        {
                            SetSelection(m_SelectedIndex + 1);
                            Event.current.Use();
                        }
                    }
                }
                else if (evt.keyCode == KeyCode.UpArrow)
                {
                    if (m_AutoCompleting)
                    {
                        m_AutoCompleteIndex = SearchService.Wrap(m_AutoCompleteIndex - 1, m_AutoCompleteMaxIndex);
                        Event.current.Use();
                    }
                    else
                    {
                        if (m_SelectedIndex >= 0)
                        {
                            if (SetSelection(m_SelectedIndex - 1) == k_ResetSelectionIndex)
                                m_SearchBoxFocus = true;
                            Event.current.Use();
                        }
                        else if (evt.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            m_CurrentSearchEvent.useHistoryShortcut = true;
                            m_CycledSearch = SearchService.CyclePreviousSearch(+1);
                            GUI.FocusControl(null);
                        }
                    }
                }
                else if (evt.keyCode == KeyCode.PageDown)
                {
                    SetSelection(m_SelectedIndex + GetDisplayItemCount() - 1);
                    Event.current.Use();
                }
                else if (evt.keyCode == KeyCode.PageUp)
                {
                    SetSelection(m_SelectedIndex - GetDisplayItemCount());
                    Event.current.Use();
                }
                else if (evt.keyCode == KeyCode.RightArrow && evt.modifiers.HasFlag(EventModifiers.Alt))
                {
                    m_CurrentSearchEvent.useActionMenuShortcut = true;
                    if (m_SelectedIndex != -1)
                    {
                        var item = m_FilteredItems.ElementAt(m_SelectedIndex);
                        var menuPositionY = (m_SelectedIndex+1) * Styles.itemRowHeight - m_ScrollPosition.y + Styles.itemRowHeight/2.0f;
                        ContextualActionPosition = new Rect(position.width - Styles.actionButtonSize, menuPositionY, 1, 1);
                        ShowItemContextualMenu(item, context, ContextualActionPosition);
                        Event.current.Use();
                    }
                }
                else if (evt.keyCode == KeyCode.LeftArrow && evt.modifiers.HasFlag(EventModifiers.Alt))
                {
                    m_CurrentSearchEvent.useFilterMenuShortcut = true;
                    PopFilterWindow();
                    Event.current.Use();
                }
                else if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return)
                {
                    if (m_AutoCompleting && m_AutoCompleteIndex != -1)
                        return;

                    var selectedIndex = m_SelectedIndex;
                    if (selectedIndex == -1 && m_FilteredItems != null && m_FilteredItems.Count > 0)
                        selectedIndex = 0;

                    if (selectedIndex != -1 && m_FilteredItems != null)
                    {
                        var item = m_FilteredItems.ElementAt(selectedIndex);
                        SearchAction action = item.provider.actions[0];
                        if (m_Context.actionQueryId != null)
                        {
                            action = SearchService.GetAction(item.provider, m_Context.actionQueryId);
                        }
                        else if (evt.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            var actionIndex = 1;
                            if (evt.modifiers.HasFlag(EventModifiers.Control))
                            {
                                actionIndex = 2;
                                if (evt.modifiers.HasFlag(EventModifiers.Shift))
                                    actionIndex = 3;
                            }
                            action = item.provider.actions[Math.Max(0, Math.Min(actionIndex, item.provider.actions.Count - 1))];
                        }

                        if (action != null)
                        {
                            Event.current.Use();
                            m_CurrentSearchEvent.endSearchWithKeyboard = true;
                            ExecuteAction(action, item, context);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    if (m_AutoCompleting)
                    {
                        m_DiscardAutoComplete = true;
                        Event.current.Use();
                    }
                    else
                    {
                        m_CurrentSearchEvent.endSearchWithKeyboard = true;
                        CloseSearchWindow();
                        Event.current.Use();
                    }
                }
                else if (evt.keyCode == KeyCode.F1)
                {
                    SetSearchText("?");
                }
                else
                    GUI.FocusControl(k_QuickSearchBoxName);

                if (prev != m_SelectedIndex)
                {
                    m_FocusSelectedItem = true;
                    Repaint();
                }
            }

            if (m_FilteredItems == null || m_FilteredItems.Count == 0)
                m_SearchBoxFocus = true;
        }

        private void CloseSearchWindow()
        {
            if (s_FocusedWindow)
                s_FocusedWindow.Focus();
            Close();
        }

        private void HandleItemEvents(int itemTotalCount, SearchContext context)
        {
            if (m_AutoCompleting && m_AutoCompleteRect.Contains(Event.current.mousePosition))
                return;

            if (Event.current.type == EventType.MouseDown)
            {
                var clickedItemIndex = (int)(Event.current.mousePosition.y / Styles.itemRowHeight);
                if (clickedItemIndex >= 0 && clickedItemIndex < itemTotalCount)
                {
                    SetSelection(clickedItemIndex);

                    if (Event.current.button == 0)
                    {
                        if ((EditorApplication.timeSinceStartup - m_ClickTime) < 0.2)
                        {
                            var item = m_FilteredItems.ElementAt(m_SelectedIndex);
                            ExecuteAction(item.provider.actions[0], item, context);
                            GUIUtility.ExitGUI();
                        }
                        EditorGUI.FocusTextInControl(k_QuickSearchBoxName);
                        Event.current.Use();
                        m_ClickTime = EditorApplication.timeSinceStartup;
                        m_PrepareDrag = true;
                    }
                    else if (Event.current.button == 1)
                    {
                        var item = m_FilteredItems.ElementAt(m_SelectedIndex);
                        var contextAction = item.provider.actions.Find(a => a.Id == SearchAction.kContextualMenuAction);
                        ContextualActionPosition = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 1, 1);
                        if (contextAction != null)
                        {
                            const bool endSearch = false;
                            ExecuteAction(contextAction, item, context, endSearch);
                        }
                        else
                        {
                            ShowItemContextualMenu(item, context, ContextualActionPosition);
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDrag && m_PrepareDrag)
            {
                if (m_FilteredItems != null && m_SelectedIndex >= 0)
                {
                    var item = m_FilteredItems.ElementAt(m_SelectedIndex);
                    if (item.provider?.startDrag != null)
                    {
                        item.provider.startDrag(item, context);
                        m_PrepareDrag = false;

                        m_CurrentSearchEvent.useDragAndDrop = true;
                        SendSearchEvent(item);

                        Event.current.Use();
                        #if UNITY_EDITOR_OSX
                        CloseSearchWindow();
                        #endif
                    }
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                m_PrepareDrag = false;
                DragAndDrop.PrepareStartDrag(); // Reset drag content
            }
        }

        private void DrawItems(SearchContext context)
        {
            UpdateScrollAreaOffset();

            context.totalItemCount = m_FilteredItems.Count;

            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
            {
                m_ScrollPosition = scrollViewScope.scrollPosition;

                var itemCount = m_FilteredItems.Count;
                var availableHeight = position.height - m_ScrollViewOffset.yMax;
                var itemSkipCount = Math.Max(0, (int)(m_ScrollPosition.y / Styles.itemRowHeight));
                var itemDisplayCount = Math.Max(0, Math.Min(itemCount, (int)(availableHeight / Styles.itemRowHeight) + 2));
                var topSpaceSkipped = itemSkipCount * Styles.itemRowHeight;

                int rowIndex = itemSkipCount;
                var limitCount = Math.Max(0, Math.Min(itemDisplayCount, itemCount - itemSkipCount));
                if (limitCount > 0)
                {
                    if (topSpaceSkipped > 0)
                        GUILayout.Space(topSpaceSkipped);

                    int thumbnailFetched = 0;
                    foreach (var item in m_FilteredItems.GetRange(itemSkipCount, limitCount))
                    {
                        try
                        {
                            DrawItem(item, context, rowIndex++, ref thumbnailFetched);
                        }
                        #if QUICKSEARCH_DEBUG
                        catch (Exception ex)
                        {
                            Debug.LogError($"itemCount={itemCount}, " +
                                           $"itemSkipCount={itemSkipCount}, " +
                                           $"limitCount={limitCount}, " +
                                           $"availableHeight={availableHeight}, " +
                                           $"itemDisplayCount={itemDisplayCount}, " +
                                           $"m_SelectedIndex={m_SelectedIndex}, " +
                                           $"m_ScrollViewOffset.yMax={m_ScrollViewOffset.yMax}, " +
                                           $"rowIndex={rowIndex-1}");
                            Debug.LogException(ex);
                        }
                        #else
                        catch
                        {
                            // ignored
                        }
                        #endif
                    }

                    var bottomSpaceSkipped = (itemCount - rowIndex) * Styles.itemRowHeight;
                    if (bottomSpaceSkipped > 0)
                        GUILayout.Space(bottomSpaceSkipped);

                    HandleItemEvents(itemCount, context);

                    // Fix selected index display if out of virtual scrolling area
                    if (Event.current.type == EventType.Repaint && m_FocusSelectedItem && m_SelectedIndex >= 0)
                    {
                        ScrollToItem(itemSkipCount + 1, itemSkipCount + itemDisplayCount - 2, m_SelectedIndex);
                        m_FocusSelectedItem = false;
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(m_Context.searchText.Trim()))
                    {
                        GUILayout.Box(string.Format(s_Helpme, m_SearchTopic), Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                    else if (m_Context.isActionQuery)
                    {
                        GUILayout.Box("Waiting for a command...", Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                    else
                    {
                        GUILayout.Box("No result for query \"" + m_Context.searchText + "\"\n" + "Try something else?",
                                      Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                }
            }
        }

        private void ExecuteAction(SearchAction action, SearchItem item, SearchContext context, bool endSearch = true)
        {
            SendSearchEvent(item, action);

            SearchService.LastSearch = context.searchText;
            SearchService.SetRecent(item);
            action.handler(item, context);
            if ((endSearch && action.closeWindowAfterExecution) && (!SearchSettings.useDockableWindow || SearchSettings.closeWindowByDefault))
                CloseSearchWindow();
        }

        private void ScrollToItem(int start, int end, int selection)
        {
            if (start <= selection && selection < end)
                return;

            Rect projectedSelectedItemRect = new Rect(0, selection * Styles.itemRowHeight, position.width, Styles.itemRowHeight);
            if (selection < start)
            {
                m_ScrollPosition.y = Mathf.Max(0, projectedSelectedItemRect.y - 2);
                Repaint();
            }
            else if (selection > end)
            {
                Rect visibleRect = GetVisibleRect();
                m_ScrollPosition.y += (projectedSelectedItemRect.yMax - visibleRect.yMax) + 2;
                Repaint();
            }
        }

        private void UpdateScrollAreaOffset()
        {
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.height > 1)
            {
                m_ScrollViewOffset = rect;
                m_ScrollViewOffset.height += Styles.statusOffset;
            }
        }

        [UsedImplicitly]
        internal void Update()
        {
            bool repaintRequested = false;
            var timeSinceStartup = EditorApplication.timeSinceStartup;
            if (m_IsRepaintAfterTimeRequested && m_RequestRepaintAfterTime <= EditorApplication.timeSinceStartup)
            {
                m_IsRepaintAfterTimeRequested = false;
                repaintRequested = true;
            }

            if (timeSinceStartup >= m_NextBlinkTime)
            {
                m_NextBlinkTime = timeSinceStartup + 0.5;
                m_CursorBlinking = !m_CursorBlinking;
                repaintRequested = true;
            }

            if (repaintRequested)
                Repaint();
        }

        private void RequestRepaintAfterTime(double seconds)
        {
            if (!m_IsRepaintAfterTimeRequested)
            {
                m_IsRepaintAfterTimeRequested = true;
                m_RequestRepaintAfterTime = EditorApplication.timeSinceStartup + seconds;
            }
        }

        private Rect DrawToolbar(SearchContext context)
        {
            if (context == null)
                return Rect.zero;

            Rect searchTextRect = Rect.zero;
            GUILayout.BeginHorizontal(Styles.toolbar);
            {
                var rightRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(32f), GUILayout.ExpandHeight(true));
                if (EditorGUI.DropdownButton(rightRect, Styles.filterButtonContent, FocusType.Passive, Styles.filterButton) || m_ShowFilterWindow)
                {
                    if (FilterWindow.canShow)
                    {
                        rightRect.x += 12f; rightRect.y -= 3f;
                        if (m_ShowFilterWindow)
                            rightRect.y += 30f;

                        m_ShowFilterWindow = false;
                        if (FilterWindow.ShowAtPosition(this, rightRect))
                            GUIUtility.ExitGUI();
                    }
                }

                EditorGUI.BeginChangeCheck();

                var previousSearchText = context.searchText;
                using (new BlinkCursorScope(m_CursorBlinking, new Color(0, 0, 0, 0.01f)))
                {
                    var userSearchQuery = context.searchText;
                    if (!String.IsNullOrEmpty(m_CycledSearch) && (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
                    {
                        userSearchQuery = m_CycledSearch;
                        m_CycledSearch = null;
                        m_SearchBoxFocus = true;
                        GUI.changed = true;
                    }

                    GUI.SetNextControlName(k_QuickSearchBoxName);
                    context.searchText = GUILayout.TextField(userSearchQuery, Styles.searchField,
                        GUILayout.MaxWidth(position.width - 80), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    m_SearchBoxControlID = GUIUtility.keyboardControl;
                    searchTextRect = GUILayoutUtility.GetLastRect();
                }

                if (String.IsNullOrEmpty(context.searchText))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextArea(GUILayoutUtility.GetLastRect(), $"search {m_SearchTopic}...", Styles.placeholderTextStyle);
                    EditorGUI.EndDisabledGroup();
                }
                if (!String.IsNullOrEmpty(context.searchText))
                {
                    if (GUILayout.Button(Icons.clear, Styles.searchFieldClear, GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        m_DiscardAutoComplete = false;
                        context.searchText = "";
                        GUI.changed = true;
                        GUI.FocusControl(null);
                    }
                }

                if (String.Compare(previousSearchText, context.searchText, StringComparison.Ordinal) != 0 || m_FilteredItems == null)
                {
                    SetSelection(k_ResetSelectionIndex);
                    DebouncedRefresh();
                }

                #if QUICKSEARCH_DEBUG
                DrawDebugTools();
                #endif
            }
            GUILayout.EndHorizontal();

            return searchTextRect;
        }

        private void DebouncedRefresh()
        {
            var currentTime = EditorApplication.timeSinceStartup;
            if (m_DebounceTime != 0 && currentTime - m_DebounceTime > 0.100)
            {
                Refresh();
                m_DebounceTime = 0;
            }
            else
            {
                if (m_DebounceTime == 0)
                    m_DebounceTime = currentTime;

                #if QUICKSEARCH_DEBUG && UNITY_2019_1_OR_NEWER
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Debouncing {0}", m_Context.searchText);
                #endif
                EditorApplication.delayCall -= DebouncedRefresh;
                EditorApplication.delayCall += DebouncedRefresh;
            }
        }

        private Rect GetVisibleRect()
        {
            Rect visibleRect = position;
            visibleRect.x = m_ScrollPosition.x;
            visibleRect.y = m_ScrollPosition.y;
            visibleRect.height -= m_ScrollViewOffset.yMax;
            return visibleRect;
        }

        private void DrawItem(SearchItem item, SearchContext context, int index, ref int thumbnailFetched)
        {
            var bgStyle = index % 2 == 0 ? Styles.itemBackground1 : Styles.itemBackground2;
            if (m_SelectedIndex == index)
                bgStyle = Styles.selectedItemBackground;

            using (new EditorGUILayout.HorizontalScope(bgStyle))
            {
                DrawThumbnail(item, context, ref thumbnailFetched);

                using (new EditorGUILayout.VerticalScope())
                {
                    var maxWidth = position.width - Styles.actionButtonSize - Styles.itemPreviewSize - Styles.itemRowSpacing;
                    var textMaxWidthLayoutOption = GUILayout.MaxWidth(maxWidth);
                    GUILayout.Label(item.provider.fetchLabel(item, context), m_SelectedIndex == index ? Styles.selectedItemLabel : Styles.itemLabel, textMaxWidthLayoutOption);
                    GUILayout.Label(FormatDescription(item, context, maxWidth), m_SelectedIndex == index ? Styles.selectedItemDescription : Styles.itemDescription, textMaxWidthLayoutOption);
                }

                if (item.provider.actions.Count > 1)
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(Styles.moreActionsContent, Styles.actionButton))
                    {
                        ShowItemContextualMenu(item, context);
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        private LinkedList<SearchItem> m_ItemPreviewCache = new LinkedList<SearchItem>();
        private void DrawThumbnail(SearchItem item, SearchContext context, ref int previewFetchedCount, int maxThumbnailFetchPerRepaint = 1, int maxItemPreviewCachedCount = 25)
        {
            Texture2D thumbnail = null;
            if (Event.current.type == EventType.Repaint)
            {
                if (SearchSettings.fetchPreview)
                {
                    thumbnail = item.preview;
                    var shouldFetchPreview = !thumbnail && item.provider.fetchPreview != null;
                    if (shouldFetchPreview && previewFetchedCount < maxThumbnailFetchPerRepaint)
                    {
                        if (m_ItemPreviewCache.Count > maxItemPreviewCachedCount)
                        {
                            m_ItemPreviewCache.First().preview = null;
                            m_ItemPreviewCache.RemoveFirst();
                        }

                        var previewSize = new Vector2(Styles.preview.fixedWidth, Styles.preview.fixedHeight);
                        thumbnail = item.provider.fetchPreview(item, context, previewSize, FetchPreviewOptions.Preview2D);
                        if (thumbnail)
                        {
                            previewFetchedCount++;
                            item.preview = thumbnail;
                            m_ItemPreviewCache.AddLast(item);
                        }
                    }
                    else if (shouldFetchPreview && previewFetchedCount == maxThumbnailFetchPerRepaint)
                    {
                        previewFetchedCount++;
                        RequestRepaintAfterTime(0.3f);
                    }
                }
                
                if (!thumbnail)
                {
                    thumbnail = item.thumbnail;
                    if (!thumbnail && item.provider.fetchThumbnail != null)
                    {
                        thumbnail = item.provider.fetchThumbnail(item, context);
                        if (thumbnail)
                            item.thumbnail = thumbnail;
                    }
                }
            }
            GUILayout.Label(thumbnail ?? Icons.quicksearch, Styles.preview);
        }

        private static string CleanString(string s)
        {
            return s.Replace('_', ' ')
                    .Replace('.', ' ')
                    .Replace('-', ' ');
        }

        private static int s_GUIContentPoolIndex = 0;
        private static readonly GUIContent[] s_GUIContentPool = new GUIContent[100];
        private static GUIContent TakeContent(string text = null, string tooltip = null, Texture2D thumbnail = null)
        {
            GUIContent content = s_GUIContentPool[s_GUIContentPoolIndex];
            if (content == null)
                s_GUIContentPool[s_GUIContentPoolIndex] = content = new GUIContent(text, thumbnail, tooltip);
            else
            {
                content.text = text;
                content.tooltip = tooltip;
                content.image = thumbnail;
            }

            SearchService.Wrap(s_GUIContentPoolIndex + 1, s_GUIContentPool.Length);
            return content;
        }

        private static GUIContent FormatDescription(SearchItem item, SearchContext context, float availableSpace)
        {
            var desc = item.description ?? item.provider.fetchDescription(item, context);
            var content = TakeContent(desc);
            if (item.descriptionFormat == SearchItemDescriptionFormat.None || Event.current.type != EventType.Repaint)
                return content;

            var truncatedDesc = desc;
            var truncated = false;
            if (item.descriptionFormat.HasFlag(SearchItemDescriptionFormat.Ellipsis))
            {
                int maxCharLength = Utils.GetNumCharactersThatFitWithinWidth(Styles.itemDescription, truncatedDesc + "...", availableSpace);
                if (maxCharLength < 0)
                    maxCharLength = truncatedDesc.Length;
                truncated = desc.Length > maxCharLength;
                if (truncated)
                {
                    if (item.descriptionFormat.HasFlag(SearchItemDescriptionFormat.RightToLeft))
                        truncatedDesc = "..." + desc.Replace("<b>", "").Replace("</b>", "").Substring(desc.Length - maxCharLength);
                    else
                        truncatedDesc = desc.Substring(0, Math.Min(maxCharLength, desc.Length)) + "...";
                }
            }

            if (item.descriptionFormat.HasFlag(SearchItemDescriptionFormat.Highlight))
            {
                var parts = context.searchQuery.Split('*', ' ', '.').Where(p => p.Length > 2);
                foreach (var p in parts)
                    truncatedDesc = Regex.Replace(truncatedDesc, Regex.Escape(p), string.Format(Styles.highlightedTextColorFormat, "$0"), RegexOptions.IgnoreCase);
            }
            else if (item.descriptionFormat.HasFlag(SearchItemDescriptionFormat.FuzzyHighlight))
            {
                long score = 1;
                List<int> matches = new List<int>();
                var sq = CleanString(context.searchQuery.ToLowerInvariant());
                if (FuzzySearch.FuzzyMatch(sq, CleanString(truncatedDesc), ref score, matches))
                    truncatedDesc = RichTextFormatter.FormatSuggestionTitle(truncatedDesc, matches);
            }

            content.text = truncatedDesc;
            if (truncated)
                content.tooltip = desc;

            return content;
        }

        private void ShowItemContextualMenu(SearchItem item, SearchContext context, Rect position = default)
        {
            var menu = new GenericMenu();
            int i = 0;
            foreach (var action in item.provider.actions)
            {
                var itemName = action.content.tooltip;
                if (i == 0)
                {
                    itemName += " _enter";
                }
                else if (i == 1)
                {
                    itemName += " _&enter";
                }
                else if (i == 2)
                {
                    itemName += " _&%enter";
                }
                else if (i == 3)
                {
                    itemName += " _&%#enter";
                }
                menu.AddItem(new GUIContent(itemName, action.content.image), false, () => ExecuteAction(action, item, context));
                ++i;
            }

            if (position == default)
                menu.ShowAsContext();
            else
                menu.DropDown(position);
        }

        public static void OpenWithContextualProvider(string providerId)
        {
            if (SearchSettings.useDockableWindow)
            {
                Debug.LogWarning("Contextual Quick Search cannot be used when the dockable quick search is enabled");
                OpenQuickSearch();
                return;
            }

            var provider = SearchService.Providers.Find(p => p.name.id == providerId);
            if (provider == null)
            {
                Debug.LogWarning("Quick Search Cannot find search provider with id: " + providerId);
                OpenQuickSearch();
                return;
            }
            SearchService.Filter.ResetFilter(false, true);
            SearchService.Filter.SetFilter(true, providerId);
            var toolWindow = ShowWindow();
            toolWindow.m_SearchTopic = provider.name.displayName.ToLower();
            toolWindow.UpdateWindowTitle();
        }

        #if UNITY_2019_1_OR_NEWER
        [UsedImplicitly, Shortcut("Help/Quick Search Contextual", KeyCode.C, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        #endif
        public static void OpenContextual()
        {
            if (SearchSettings.useDockableWindow)
            {
                Debug.LogWarning("Contextual Quick Search cannot be used when the dockable quick search is enabled");
                return;
            }

            var contextualProviders = SearchService.Providers
                .Where(searchProvider => searchProvider.active && searchProvider.isEnabledForContextualSearch != null && searchProvider.isEnabledForContextualSearch()).ToArray();
            if (contextualProviders.Length == 0)
            {
                OpenQuickSearch();
                return;
            }

            SearchService.Filter.ResetFilter(false, true);
            foreach (var searchProvider in contextualProviders)
                SearchService.Filter.SetFilter(true, searchProvider.name.id, null, true);

            ShowWindow();
        }

        public static QuickSearchTool ShowWindow()
        {
            s_FocusedWindow = focusedWindow;

            var windowSize = new Vector2(650, 440);

            QuickSearchTool qsWindow;
            if (!SearchSettings.useDockableWindow)
            {
                qsWindow = CreateInstance<QuickSearchTool>();
                qsWindow.ShowDropDown(windowSize);
            }
            else
            {
                qsWindow = GetWindow<QuickSearchTool>();
                qsWindow.Show();
            }

            qsWindow.m_SearchTopic = "anything";
            // Ensure we won't send events while doing a domain reload.
            qsWindow.m_SendAnalyticsEvent = true;
            qsWindow.Focus();

            return qsWindow;
        }

        public static bool IsFocusedWindowTypeName(string focusWindowName)
        {
            return focusedWindow != null && focusedWindow.GetType().ToString().EndsWith("." + focusWindowName);
        }

        #region Text AutoComplete
        private Rect m_AutoCompleteRect;
        private bool m_DiscardAutoComplete = false;
        private bool m_AutoCompleting = false;
        private int m_AutoCompleteIndex = -1;
        private int m_AutoCompleteMaxIndex = 0;
        private string m_AutoCompleteLastInput;
        private List<string> m_CacheCheckList = null;
        private string TextFieldAutoComplete(Rect position, string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
        {
            if (input.Length <= 0)
                return input;

            string rst = input;
            if (m_AutoCompleteLastInput != input) // another field.
            {
                // Update cache
                m_AutoCompleteLastInput = input;

                List<string> uniqueSrc = new List<string>(new HashSet<string>(source)); // remove duplicate
                int srcCnt = uniqueSrc.Count;
                m_CacheCheckList = new List<string>(System.Math.Min(maxShownCount, srcCnt)); // optimize memory alloc

                // Start with - slow
                for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                {
                    if (uniqueSrc[i].ToLower().StartsWith(input.ToLower()))
                    {
                        m_CacheCheckList.Add(uniqueSrc[i]);
                        uniqueSrc.RemoveAt(i);
                        srcCnt--;
                        i--;
                    }
                }

                // Contains - very slow
                if (m_CacheCheckList.Count == 0)
                {
                    for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                    {
                        if (uniqueSrc[i].ToLower().Contains(input.ToLower()))
                        {
                            m_CacheCheckList.Add(uniqueSrc[i]);
                            uniqueSrc.RemoveAt(i);
                            srcCnt--;
                            i--;
                        }
                    }
                }

                // Levenshtein Distance - very very slow.
                if (levenshteinDistance > 0f && // only developer request
                    input.Length > 3 && // 3 characters on input, hidden value to avoid doing too early.
                    m_CacheCheckList.Count < maxShownCount) // have some empty space for matching.
                {
                    levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
                    string keywords = input.ToLower();
                    for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                    {
                        int distance = Utils.LevenshteinDistance(uniqueSrc[i], keywords, caseSensitive: false);
                        bool closeEnough = (int)(levenshteinDistance * uniqueSrc[i].Length) > distance;
                        if (closeEnough)
                        {
                            m_CacheCheckList.Add(uniqueSrc[i]);
                            uniqueSrc.RemoveAt(i);
                            srcCnt--;
                            i--;
                        }
                    }
                }
            }

            if (m_CacheCheckList.Count == 0)
                return null;

            // Draw recommend keyword(s)
            if (m_CacheCheckList.Count > 0)
            {
                int cnt = m_CacheCheckList.Count;
                float height = cnt * EditorStyles.toolbarDropDown.fixedHeight;
                Rect area = position;
                area = new Rect(area.x, area.y, area.width, height);
                GUI.depth -= 10;
                GUI.BeginClip(area);
                Rect line = new Rect(0, 0, area.width, EditorStyles.toolbarDropDown.fixedHeight);

                for (int i = 0; i < cnt; i++)
                {
                    var selected = i == m_AutoCompleteIndex;
                    if (selected)
                    {
                        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
                        {
                            Event.current.Use();
                            GUI.changed = true;
                            return m_CacheCheckList[i];
                        }
                        GUI.DrawTexture(line, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false, 0, Styles.textAutoCompleteSelectedColor, 0f, 1.0f);
                    }
                    else
                    {
                        GUI.DrawTexture(line, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false, 0, Styles.textAutoCompleteBgColor, 0f, 1.0f);
                    }

                    if (GUI.Button(line, m_CacheCheckList[i], selected ? Styles.selectedItemLabel : Styles.itemLabel))
                    {
                        rst = m_CacheCheckList[i];
                        GUI.changed = true;
                    }

                    line.y += line.height;
                }
                GUI.EndClip();
                GUI.depth += 10;
            }
            return rst;
        }

        #endregion

        #if UNITY_2019_3_OR_NEWER
        [UsedImplicitly, CommandHandler(nameof(OpenQuickSearch))]
        private static void OpenQuickSearchCommand(CommandExecuteContext c)
        {
            OpenQuickSearch();
        }

        [InitializeOnLoadMethod]
        private static void OpenQuickSearchFirstUse()
        {
            var quickSearchFirstUseTokenPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "Library", "~quicksearch.new"));
            if (System.IO.File.Exists(quickSearchFirstUseTokenPath))
            {
                System.IO.File.Delete(quickSearchFirstUseTokenPath);
                EditorApplication.delayCall += OpenQuickSearch;
            }
        }
        #endif

        #if UNITY_2019_1_OR_NEWER
        [Shortcut("Help/Quick Search", KeyCode.O, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        #endif
        #if !UNITY_2019_3_OR_NEWER
        [MenuItem("Help/Quick Search &'", priority = 9000)]
        #endif
        [UsedImplicitly]
        private static void OpenQuickSearch()
        {
            SearchService.LoadFilters();
            ShowWindow();
        }

        #if QUICKSEARCH_DEBUG
        [UsedImplicitly, MenuItem("Tools/Clear Editor Preferences")]
        private static void ClearPreferences()
        {
            EditorPrefs.DeleteAll();
        }

        private void DrawDebugTools()
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                SearchService.Refresh();
                Refresh();
            }
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                SearchService.SaveGlobalSettings();
            }
            if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
            {
                SearchService.Reset();
                Refresh();
            }
        }
        #endif
    }
}
