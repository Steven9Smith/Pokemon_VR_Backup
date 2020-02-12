# Search Providers

Quick Search comes with a lot of providers that indexed multiple parts of Unity. See the [api](api.md) section if you want to create your own provider.

## Assets

All assets in the current project are available for search. We use our own asset indexer (different than the AsetDatabase or the Project Browser) but that yields results way faster.

From an asset item you can apply the following actions:

- Select the asset (in the Project browser)
- Open the asset (using an external editor if necessary)
- Show in Explorer (or Finder)

![asset provider actions](Images/asset_provider_actions.png)

You can disable packages Search using the Asset provider specific filter:

![asset filter](Images/asset_provider_filters.png)

You can also support search by GUIDs (instanceIDs)

### Difference from AssetDatabase Search

The Asset Provider doesn't recognize the type (ex: `t:Scripts) or label (l:Terrain) filters. But it is much more flexible:

- You can type **file extension** and it will find all assets
- It does matching against directory name
- It does partial search
- It is much faster!

### Use the AssetDatabase search

If you want to rely on the AssetDatabase search (completely bypassing our fast asset indexer) just use any AssetDatabase search tokens like `t:` or `l:`:

![autocomplete](Images/asset_provider_auto_complete.gif)

Notice how Quick Search uses Auto completion to suggest the supported filter.

### Searching the file system

If you use the `*` on a query it will perform a normal search AND a wildcard search against the file systems (allowing you to track files not indexed by the AssetDatabase).

### Searching by GUIDs

The Asset search provider supports searching with GUIDs:

![guid](Images/asset_provider_guid_search.png)

## Current Scene

All GameObjects in the current scene are available for search. Activating a scene item in the Quick Search tool will select the corresponding GameObject in the Scene.

![scene provider](Images/scene_provider_selection.png)

By default Search in Scene are done with a fuzzy search. Notice how 
`/Canvas/Score/` matches the search query `cs`. Using fuzzy search is a bit more costly than a direct search so it might be slower in bigger scenes. If you want to disable Fuzzy Searching use the Quick Search Filter Window:

![scene provider](Images/scene_provider_fuzzy_settings.png)

## Menu

Each menu item in Unity is available within Quick Search. This is especially useful to pop that elusive Test Runner or the Profiler!

![asset filter](Images/menu_provider.png)

## Settings

Each Project Setting or Preferences page is available for search. The Quick Search Tool will open the Unified Settings Window at the required page.

![asset filter](Images/settings_provider.png)

## Packages

The Package Search Provider allows you to search for any existing packages and install, update it or remove it.

![asset filter](Images/pkg_provider.png)


## Online Search Providers

We have a `SearchProvider` that allows to search various Unity websites. Using this provider will open your default browser at a specific Unity page and perform a search and display some results. You can search the following websites and it is very easy to add new web search providers:

- [answers.unity.com](answers.unity.com)
- [The Official Unity Documentation](docs.unity3d.com/Manual)
- [Scripting Reference](docs.unity3d.com/ScriptingReference)
- [The Mighty Unity Asset Store](assetstore.unity.com)

![store](Images/online_search.gif)

# Explicit Providers

Explicit providers are **only** queried for when their *filter id* is used. Thus when doing a normal search their results won't appear in the item list.

![explicit](Images/explicit_providers.png)

## Calculator (=)
Using the token `=` will trigger the calculator. You can enter any expression valid in our different numerical textfields. The result will be printed directly. Selecting the item will print the result in the console and in the clipboard.

![calculator](Images/calculator_provider.gif)

## Static API (#)

This clever search provider indexed all **public static API of Unity** and make them available for *execution*:

![calculator](Images/static_api.gif)

## Command Query (>)

Command Query is a special provider that can be triggered using `>`. It works backward to the normal search workflow: instead of typing a search query and then select a specific action to apply on an item, you type a **specific action** (with autocompletion) and it will filter **only** items supporting this specific action.

![command_query](Images/command_query.gif)
