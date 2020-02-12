using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// UI Module for category selection UI.
    /// </summary>
    internal class CategoryPickerEditor
    {
        private class CategoryRow
        {
            public List<CategoryDefinition> categories;

            public CategoryRow()
            {
                categories = new List<CategoryDefinition>();
            }
        }

        private List<CategoryRow> m_WrappableCategoryRows = new List<CategoryRow>();
        private List<CategoryDefinition> m_CategorySearchResults = new List<CategoryDefinition>();
        private List<CategoryDefinition> m_existingItemCategories;

        private InventoryCatalog m_Catalog;
        private Rect m_CategoryItemsRect = new Rect();
        private string m_CategorySearchString = string.Empty;
        private string m_CategorySearchStringPrevious = string.Empty;
        private SearchField m_CategorySearchField = new SearchField();

        private Rect m_SuggestRect;
        private Vector2 m_CategorySearchSuggestScrollPosition = Vector2.zero;
        private int m_CategorySuggestSelectedIndex = -1;
        private bool m_UsedScrollWheelInSuggestBox = false;

        internal CategoryPickerEditor(InventoryCatalog catalog)
        {
            m_Catalog = catalog;
        }

        /// <summary>
        /// Draws category selection search bar and selected categories.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPicker(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            m_existingItemCategories = EditorAPIHelper.GetGameItemDefinitionCategories(gameItemDefinition);
            DrawCategoriesDetail(gameItemDefinition, catalogCategories);
        }

        /// <summary>
        /// Draws category search suggestion view. NOTE: This needs to be the last GUI call
        /// in the given window otherwise other elements will be drawn over it.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPickerPopup(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            DrawCategorySearchSuggest(gameItemDefinition, catalogCategories);
            HandleCategorySearchInput(gameItemDefinition, catalogCategories);
        }

        /// <summary>
        /// Resets category search string.
        /// </summary>
        public void ResetCategorySearch(bool takeFocus = false)
        {
            m_CategorySuggestSelectedIndex = -1;
            m_CategorySearchString = string.Empty;

            if (takeFocus)
            {
                // do both of these - the first one just makes sure the next control doesn't get focused, the second one makes sure the text is being edited
                EditorGUI.FocusTextInControl("search field");
                m_CategorySearchField.SetFocus();
            }

            m_UsedScrollWheelInSuggestBox = false;
        }

        private void DrawCategoriesDetail(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            EditorGUILayout.LabelField("Categories", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                EditorGUILayout.LabelField("Assign existing categories or create new ones.");

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            m_CategorySearchString = m_CategorySearchField.OnGUI(m_CategorySearchString);

                            if (m_CategorySearchStringPrevious != m_CategorySearchString)
                            {
                                UpdateCategorySuggestions(gameItemDefinition, catalogCategories);
                            }
                            m_CategorySearchStringPrevious = m_CategorySearchString;

                            // only show the Add button if:
                            //  • we are searching
                            //  • there are no suggestions found

                            if (!string.IsNullOrEmpty(m_CategorySearchString) && m_CategorySearchResults.Count <= 0)
                            {
                                if (GUILayout.Button("Add", GUILayout.Width(CategoryPickerStyles.categoryAddButtonWidth)))
                                {
                                    // same as if user presses Enter or Return

                                    CreateAndAssignCategoryFromSearchField(gameItemDefinition, catalogCategories);
                                }
                            }
                        }

                        EditorGUILayout.Space();

                        // dimensions should be calculated during Repaint because during Layout they aren't calculated yet
                        if (Event.current.type == EventType.Repaint)
                        {
                            m_SuggestRect = GUILayoutUtility.GetLastRect();
                            m_SuggestRect.x += 24;
                            m_SuggestRect.width -= 40;
                            m_SuggestRect.height = 220;

                            m_CategoryItemsRect = GUILayoutUtility.GetLastRect();
                            m_CategoryItemsRect.x += 12f;
                            m_CategoryItemsRect.y += 18f;

                            RecalculateCategoryBoxHeight(gameItemDefinition);
                        }

                        // don't modify a collection while iterating through it
                        CategoryDefinition categoryToRemove = null;

                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                        {
                            // make enough room
                            GUILayout.Space(m_CategoryItemsRect.height);

                            // inside this vertical area, we cannot use GUILayout anymore because
                            // for/while inside GUILayout horizontal and vertical scopes will
                            // generate errors between Layout and Repaint events (chicken/egg problem)

                            for (int categoryRowIndex = 0; categoryRowIndex < m_WrappableCategoryRows.Count; categoryRowIndex++)
                            {
                                CategoryRow row = m_WrappableCategoryRows[categoryRowIndex];

                                float rowHeight = CategoryPickerStyles.categoryListItemStyle.CalcHeight(new GUIContent("lorem ipsum"), 1000f);

                                Rect rowRect = new Rect(m_CategoryItemsRect);
                                rowRect.height = rowHeight;
                                rowRect.y += categoryRowIndex * rowHeight;
                                rowRect.y += categoryRowIndex * CategoryPickerStyles.categoryItemMargin;

                                float curX = 0f;

                                foreach (CategoryDefinition category in row.categories)
                                {
                                    Vector2 categoryNameContentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));

                                    Rect itemRect = new Rect(rowRect);
                                    itemRect.x += curX;
                                    itemRect.width = categoryNameContentSize.x + CategoryPickerStyles.categoryListItemStyle.padding.horizontal;

                                    Rect categoryDeleteButtonRect = new Rect(itemRect);
                                    // adjust the X rect over to the right side
                                    categoryDeleteButtonRect.x = itemRect.x + itemRect.width - CategoryPickerStyles.categoryRemoveButtonSpaceWidth;
                                    categoryDeleteButtonRect.width = CategoryPickerStyles.categoryRemoveButtonSpaceWidth;
                                    // nudge it a bit to look better
                                    categoryDeleteButtonRect.x -= 2;
                                    categoryDeleteButtonRect.y += 4;

                                    GUI.Box(itemRect, category.displayName, CategoryPickerStyles.categoryListItemStyle);

                                    if (GUI.Button(categoryDeleteButtonRect, "<b>X</b>", GameFoundationEditorStyles.deleteButtonStyle))
                                    {
                                        categoryToRemove = category;
                                    }

                                    curX += itemRect.width + CategoryPickerStyles.categoryItemMargin;
                                }
                            }
                        }

                        if (categoryToRemove != null)
                        {
                            gameItemDefinition.RemoveCategory(categoryToRemove);
                            m_existingItemCategories = EditorAPIHelper.GetGameItemDefinitionCategories(gameItemDefinition);
                        }
                    }
                }
            }
        }

        private void DrawCategorySearchSuggest(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            // only show the search suggest window and handle input for it if...
            // - the search field is currently in focus
            // - there is text in the search field
            // - there are suggestions to show
            if (string.IsNullOrEmpty(m_CategorySearchString)) return;
            if (m_CategorySearchResults.Count <= 0) return;

            // adjust scroll position if the highlighted item is not visible
            // but if the scroll wheel is used, then obey the scroll wheel instead

            if (Event.current.type == EventType.ScrollWheel) m_UsedScrollWheelInSuggestBox = true;

            if (!m_UsedScrollWheelInSuggestBox)
            {
                float rowHeight = CategoryPickerStyles.categorySuggestItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
                float minVisibleY = m_CategorySearchSuggestScrollPosition.y;
                float maxVisibleY = m_SuggestRect.height + m_CategorySearchSuggestScrollPosition.y;
                float selectedItemTopY = rowHeight * m_CategorySuggestSelectedIndex;
                float selectedItemBottomY = selectedItemTopY + rowHeight;

                if (minVisibleY > selectedItemTopY)
                {
                    m_CategorySearchSuggestScrollPosition.Set(0, selectedItemTopY);
                }

                if (maxVisibleY < selectedItemBottomY)
                {
                    m_CategorySearchSuggestScrollPosition.Set(0, selectedItemBottomY - m_SuggestRect.height);
                }
            }

            // RENDER

            using (new GUILayout.AreaScope(m_SuggestRect, "", CategoryPickerStyles.searchSuggestAreaStyle))
            {
                using (var scrollViewScope = new GUILayout.ScrollViewScope(m_CategorySearchSuggestScrollPosition, false, true))
                {
                    m_CategorySearchSuggestScrollPosition = scrollViewScope.scrollPosition;

                    for (int resultIndex = 0; resultIndex < m_CategorySearchResults.Count; resultIndex++)
                    {
                        CategoryDefinition suggestedCategory = m_CategorySearchResults[resultIndex];

                        // use the normal style, unless this is the highlighted item, in which case use the highlighted style
                        GUIStyle style = resultIndex == m_CategorySuggestSelectedIndex ? CategoryPickerStyles.categorySuggestItemStyleSelected : CategoryPickerStyles.categorySuggestItemStyle;

                        if (GUILayout.Button(suggestedCategory.displayName, style, GUILayout.ExpandWidth(true)))
                        {
                            AssignCategory(gameItemDefinition, suggestedCategory);
                            ResetCategorySearch(takeFocus: true);
                            UpdateCategorySuggestions(gameItemDefinition, catalogCategories);
                            RecalculateCategoryBoxHeight(gameItemDefinition);
                        }
                    }
                }
            }
        }

        private void HandleCategorySearchInput(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            if (string.IsNullOrEmpty(m_CategorySearchString)) return;

            if (Event.current.type == EventType.KeyUp)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.UpArrow:
                        if (m_CategorySearchResults.Count > 0)
                        {
                            Event.current.Use();

                            m_CategorySuggestSelectedIndex -= 1;
                            m_UsedScrollWheelInSuggestBox = false;
                        }
                        break;

                    case KeyCode.DownArrow:
                        if (m_CategorySearchResults.Count > 0)
                        {
                            Event.current.Use();
                            m_CategorySuggestSelectedIndex += 1;
                            m_UsedScrollWheelInSuggestBox = false;
                        }
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                    case KeyCode.Tab:
                        Event.current.Use();

                        if (m_CategorySearchResults.Count > 0)
                        {
                            if (m_CategorySuggestSelectedIndex >= 0)
                            {
                                // if there are results and one is selected, then assign it
                                AssignCategory(gameItemDefinition, m_CategorySearchResults[m_CategorySuggestSelectedIndex]);
                                RecalculateCategoryBoxHeight(gameItemDefinition);
                            }
                            else
                            {
                                // if there are results but none are selected, then do nothing
                            }
                        }
                        else if (Event.current.keyCode != KeyCode.Tab)
                        {
                            // same as if "Add" is clicked
                            // if there are no suggestions but there is search string, then create a new category
                            // but it's probably not expected when tab key is used, so we'll exclude that one

                            CreateAndAssignCategoryFromSearchField(gameItemDefinition, catalogCategories);
                        }

                        ResetCategorySearch(takeFocus: true);
                        UpdateCategorySuggestions(gameItemDefinition, catalogCategories);
                        break;

                    case KeyCode.Escape:
                        Event.current.Use();
                        ResetCategorySearch();
                        UpdateCategorySuggestions(gameItemDefinition, catalogCategories);
                        break;

                    default:
                        break;
                }

                CorrectCategorySearchSuggestSelectedIndex();
            }
        }

        private void CreateAndAssignCategoryFromSearchField(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            // don't allow creation of duplicate displayNames here
            // you can still do it in the main category editor
            if (catalogCategories == null || catalogCategories.Any(category => category.displayName == m_CategorySearchString)) return;

            string categoryId = CollectionEditorTools.CraftUniqueId(m_CategorySearchString, new HashSet<string>(catalogCategories.Select(category => category.id)));

            CategoryDefinition newCategory = EditorAPIHelper.CreateCategoryDefinition(categoryId, m_CategorySearchString);
            if (newCategory != null)
            {
                m_Catalog.AddCategory(newCategory);
                catalogCategories = EditorAPIHelper.GetInventoryCatalogCategoriesList();
                AssignCategory(gameItemDefinition, newCategory);

                // Refresh settings with new category
                RecalculateCategoryBoxHeight(gameItemDefinition);
                ResetCategorySearch(takeFocus: true);
                UpdateCategorySuggestions(gameItemDefinition, catalogCategories);
                CategoryFilterEditor.RefreshSidebarCategoryFilterList(catalogCategories);
            }
        }

        private void RecalculateCategoryBoxHeight(GameItemDefinition gameItemDefinition)
        {
            float currentRowContentWidth = 0f;

            m_WrappableCategoryRows = new List<CategoryRow>() { new CategoryRow() };

            if (m_existingItemCategories != null)
            {
                foreach (CategoryDefinition category in m_existingItemCategories)
                {
                    Vector2 contentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));
                    contentSize.x += CategoryPickerStyles.categoryListItemStyle.padding.horizontal + CategoryPickerStyles.categoryRemoveButtonSpaceWidth;

                    if (currentRowContentWidth + contentSize.x > m_CategoryItemsRect.width)
                    {
                        m_WrappableCategoryRows.Add(new CategoryRow());
                        currentRowContentWidth = 0f;
                    }

                    m_WrappableCategoryRows.Last().categories.Add(category);
                    currentRowContentWidth += contentSize.x;
                }
            }

            m_CategoryItemsRect.height = m_WrappableCategoryRows.Count * CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
            m_CategoryItemsRect.height += (m_WrappableCategoryRows.Count - 1) * CategoryPickerStyles.categoryItemMargin;
        }

        private void UpdateCategorySuggestions(GameItemDefinition gameItemDefinition, List<CategoryDefinition> catalogCategories)
        {
            if (string.IsNullOrEmpty(m_CategorySearchString) || catalogCategories == null)
            {
                m_CategorySearchResults = new List<CategoryDefinition>();
                m_CategorySuggestSelectedIndex = -1;
                return;
            }

            List<CategoryDefinition> potentialMatches = catalogCategories.FindAll(cat =>
                cat.displayName.ToLowerInvariant().Contains(m_CategorySearchString.ToLowerInvariant())
            );
            m_CategorySearchResults = potentialMatches
                .Where(potentialCategory => {
                    if (m_existingItemCategories == null)
                    {
                        return false;
                    }
                    return m_existingItemCategories.All(existingCategory => existingCategory != potentialCategory);
                }).ToList();

            CorrectCategorySearchSuggestSelectedIndex();
        }

        private void CorrectCategorySearchSuggestSelectedIndex()
        {
            if (m_CategorySearchResults.Count <= 0)
            {
                m_CategorySuggestSelectedIndex = -1;
            }
            else if (m_CategorySuggestSelectedIndex < 0)
            {
                m_CategorySuggestSelectedIndex = m_CategorySearchResults.Count - 1;
            }
            else if (m_CategorySuggestSelectedIndex >= m_CategorySearchResults.Count)
            {
                m_CategorySuggestSelectedIndex = 0;
            }
        }

        private void AssignCategory(GameItemDefinition gameItemDefinition, CategoryDefinition addCategory)
        {
            if (gameItemDefinition != null && addCategory != null)
            {
                gameItemDefinition.AddCategory(addCategory);
                m_existingItemCategories = EditorAPIHelper.GetGameItemDefinitionCategories(gameItemDefinition);
            }
        }
    }
}
