## [1.4.1] - 2019-09-03
- Quick Search is now a verified package.
- [UX] Add UIElements experimental search provider
- [FIX] Add to the asset search provider type filter all ScriptableObject types
- [FIX] Fix Asset store URL
- [DOC] Document more public APIs
- [API] Add programming hooks to the scene search provider

## [1.3.2-preview] - 2019-07-24
- [UX] Ignore filter ID casing in search text queries (i.e. H:camera will works like h:camera)
- [FIX] Fix division by 0 when searching with no search provider enabled.
- [API] Expose new async API and add more documentation XML tags.

## [1.3.1-preview] - 2019-07-18
- [UX] Show the async spinner while the scene and asset indexes are being built.
- [UX] Removed the objects limit for the scene object provider.
- [UX] Progressively show search item previews (preventing the UI to block when fetching large previews)
- [FIX] Use the async fetchItems for the asset search provider returning result faster when using file system wildcard * (i.e. *.png)
- [FIX] Optimize the scene object provider to build all indexes asynchronously.
- [FIX] Optimize the insert and sort algorithm.
- [API] Add new API `SearchProvider.fetchPreview` to fetch larger and slower preview of search item. `fetchThumbnail` is used to show a thumbnail and must be fast, then `fetchPreview` is called progressively to display a better and larger preview.

## [1.3.0-preview] - 2019-07-12
- [UX] Add the ability to enable or disable completely a search provider in the user preferences.
- [UX] Add experimental Asset Store provider (disabled by default, see Quick Search preferences).
- [UX] Add an experimental log search provider (disabled by default, see Quick Search preferences).
- [UX] Add a spinner when search is in progress.
- [FIX] Do not fetch scene root objects for invalid or unloaded scenes.
- [FIX] Delay the track selection, keeping only the last selection to actually be pinged.
- [FIX] Add support for one letter word indexing (i.e. Searching for "Rock A" should match asset Assets/Rock_A.png").
- [API] Removed the old asynchronous API.
- [API] Port the ESS provider example to the new asynchronous API.
- [API] Port the AssetStore provider example to the new asynchronous API.
- [API] Add asynchronous SearchProvider.fetchItems API.

## [1.2.9-preview] - 2019-07-04
- [UX] Show additional timing information in tooltips.
- [FIX] Fix QUICKSEARCH_EXAMPLES and QUICKSEARCH_DEBUG compilation.
- [FIX] Fix asset search provider performance regression at initialization.
- [DOC] Update documentation with all the new workflows and apis.

## [1.2.8-preview] - 2019-06-30
- [FIX] Add more resilience when a domain reload happens while being searching indexes in another thread.
- [FIX] Add search indexing to the scene search provider, providing better results.

## [1.2.7-preview] - 2019-06-19
- [FIX] Fix asset refreshing after quitting the application, causing Unity to freeze.
- [FIX] Fix fetching scene object label when component are missing

## [1.2.6-preview] - 2019-06-14
- [FIX] Hot fix for crash on OSX when dragging an element.

## [1.2.5-preview] - 2019-06-14
- [UX] Search scene objects by component and contained sub scene name.
- [FIX] Optimize GetAllDerivedTypes usage based on the comment <https://forum.unity.com/threads/quick-search-preview.635863/page-4#post-4643902>
- [FIX] Improve item description formatting.
- [CI] Update yamato config.

## [1.2.4-preview] - 2019-06-13
- [UX] Add support to search asset by its GUID.
- [UX] Add scene objects root container asset information.
- [UX] Add prefab instance preview for scene objects.
- [FIX] Improve search provider filtering.
- [FIX] Fix recently used item persistence.
- [FIX] Add an active loop while installing and removing packages to prevent any race conditions.

## [1.2.3-preview] - 2019-06-06
- Add a new help search provider to guide the user how to use other search providers (type ? to get a list of search help items).
- [UX] Draw some nice up and down arrows to order the search provider priority.
- [UX] Add a status bar to give some context to the user what is going on and what is filtered or not.
- [UX] Add a Reset Priorities button in the Settings window to restore all default search provider priorities if messed up for whatever reason.
- [FIX] Fix the ordering of search provider priorities in the Settings window.
- [FIX] Fix the order of explicit providers in the filter popup window.
- [FIX] Fix a rare exception when the SearchIndexer is trying to write to the temporary index DB.
- [API] Add `SearchAction.closeWindowAfterExecution` to indicate if the window should be closed after executing an action. Now an action can append to the search text and show additional search items.
- [API] Add `ISearchView` interface for `SearchContext.searchView` to give access to some functionalities of the Quick Search host window.

## [1.2.2-preview] - 2019-06-05
- Add more performance, productivity and fun!
- Add fuzzy search to scene objects search provider.
- [UX] Separate regular search providers from explicit search providers in the filter popup window.
- [UX] Make the Quick Search window default size larger.
- [UX] Make the asset indexed search the default search for the asset search provider.
- [UX] Allow the user to change the default action to be executed for search item with multiple actions.
- [UX] Add more tooltips to some controls to indicate more advance features.
- [UX] Add ... to long search item description and show the entire description as a tooltip.
- [API] Expose the SearchIndexer API to easily add new search provider with indexed data.
- [API] Add SearchProvider.isExplicitProvider to mark a provider as explicit. An explicit provider only list search result if the the search query start with its filter id. In example, # will get into the calculator search provider and # will only return static method APIs.
- [API] Add Search.customDescriptionFormatter to allow a search provider to indicate that the item description was already done and should not be done generically (used by the scene object search provider fuzzy search). 

## [1.2.1-preview] - 2019-05-29
- Various domain reload fixes

## [1.2.0-preview.1] - 2019-05-28
- Add >COMMAND to execute a specific command on a specific item. (i.e. >reveal "Assets/My.asset", will reveal in the asset in the file explorer directly)
- Add a search provider to quickly install packages.
- Add a search provider to search and invoke any static method API.
- Add Alt+Shift+C shortcut to open the quick search contextually to the selected editor window.
- Add auto completion of asset type filter (i.e. start typing t:)
- Add contextual action support to search item (i.e. opened by right clicking on an asset)
- Add index acronym search result (i.e. WL will match Assets/WidgetLibrary.cs)
- Add indexing item scoring to sort them (items with a better match are sorted first)
- Add QuickSearchTool.OpenWithContextualProvider API to open the quick search for a specific type only.
- Add selected search settings to analytics.
- Add selection tracking fo the selected search item (can be turned off in the settings)
- Add support for package indexing
- Add support to sort search provider priorities in the user preferences
- Commands using > are shown using the auto-complete drop down.
- Do not show search provider in the filter window when opened for a specific type.
- Give a higher score to recently used items so they get sorted first when searched.
- Improve filter window styling
- Improve item hovering when moving the mouse over items.
- Improved fast typing debouncing
- Improved item description highlighting of matching keywords
- Launch Quick Search the first time it was installed from the an official 19.3 release.
- Potential fix for ThreadAbortException
- Remove type sub filters to the asset provider
- Skip root items starting with a .

## [1.1.0-preview.1] - 2019-05-14
- Add a switch to let search provider fetch more results if requested (e.g. `SearchContext.wantsMore`)
- Add selection tracking to the `SearchProvider` API.
- Fix search item caching.
- Improve quick search debouncing when typing.
- Ping scene objects when the selection changes to a scene provider search item in the Quick Search window.
- Track project changes so search provider can refresh themselves.
- Update file indexes when the project content changes.
- Update UI for Northstar retheming.
  
## [1.0.9-preview.2] - 2019-05-06
- Add editor analytics
- Add fast indexing of the project assets file system.
- Add option to open the quick search in a dockable window.
- Add search service tests
- Add shortcut key bindings to the menu item descriptor.
- Add typing debouncing (to prevent searching for nothing while typing fast)
- Add yamato-CI
- Format the search item description to highlight the search terms.
- Improve documentation
- Improve the performance of searches when folder filter is selected.
- Record average fetch time of search provider and display those times in the Filter Window.
- Remove assets and scene search provider default shortcuts. They were conflicting with other core shortcuts.
- Remove duplicate items from search result.

## [1.0.1-preview.1] - 2019-02-27
- Fix ReflectionTypeLoadException exception for certain user with invalid project assemblies
- Optimize fetching results from menu, asset and scene search providers (~50x boost).

## [1.0.0-preview.2] - 2019-02-26
- Update style to work better with the Northstar branch.
- Use Alt+Left to open the filter window box.
- Use Alt+Down to cycle to next recent search.
- Navigate the filter window using the keyboard only.
- Consolidate web search providers into a single one with sub categories.
- Update documentation to include new features.
- Set the game object as the selected active game object when activated.

## [0.9.95-preview] - 2019-02-25
- Add AssetStoreProvider example
- Add HTTP asynchronous item loading by querying the asset store.
- Add onEnable/onDisable API for Search provider.
- Add Page Up and Down support to scroll the item list faster.
- Add search provider examples
- Add support for async results
- Cycle too previous search query using ALT+UpArrow
- Fix various Mac UX issues
- New icons
- Select first item by default en pressing

## [0.9.9-preview.2] - 2019-02-21
- Added drag and drop support. You can drag an item from the quick search tool to a drop target.
- Open the item contextual menu by pressing the keyboard right arrow of the selected item.
- Fixed folder entry search results when only the folder filter is selected.
- Fixed showing popup when alt-tabbing between windows.
- Add support for rich text label and description.
- Updated documentation

## [0.9.7-preview.3] - 2019-02-20
- Fixed cursor blinking
- Improved fetched scene object results

## [0.9.6-preview.1] - 2019-02-20
- Moved menu items under Help/
- Added a warning when all filters are disabled.
- Added search field cursor blinking.
- Added search field placeholder text when not search is made yet.
- Fixed a layout scoping issue when scrolling.

## [0.9.5-preview.1] - 2019-02-19
- First Version
- Search menu items
- Search project assets
- Search current scene game objects
- Search project and preference settings
- Build a search query for  Unity Answers web site
- Build a search query for the Unity Documentation web site
- Build a search query for the Unity Store web site
