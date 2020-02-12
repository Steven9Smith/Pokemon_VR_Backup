using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal static class EditorAPIHelper
    {
        internal static readonly string k_MainInventoryDefinitionId = "main";
        internal static readonly string k_WalletInventoryDefinitionId = "wallet";

        // GameItemCatalog helper methods
        internal static List<GameItemDefinition> GetGameItemCatalogGameItemDefinitionsList()
        {
            if (GameFoundationSettings.database.gameItemCatalog == null)
            {
                return null;
            }

            List<GameItemDefinition> gameItemDefinitions = new List<GameItemDefinition>();
            GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinitions(gameItemDefinitions);
            return gameItemDefinitions;
        }

        internal static void AddGameItemDefinitionToGameItemCatalog(GameItemDefinition gameItemItem)
        {
            if (GameFoundationSettings.database.gameItemCatalog != null)
            {
                GameFoundationSettings.database.gameItemCatalog.AddGameItemDefinition(gameItemItem);
            }
            else
            {
                Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " could not be added to the GameItem catalog because catalog is null");
            }
        }

        internal static void RemoveGameItemDefinitionFromGameItemCatalog(GameItemDefinition gameItemItem)
        {
            if (GameFoundationSettings.database.gameItemCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.database.gameItemCatalog.RemoveGameItemDefinition(gameItemItem);
                if (!successfullyRemoved)
                {
                    Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " was unable to be removed from gameItem catalog list.");
                }
            }
            else
            {
                Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " could not be removed from gameItem catalog because catalog is null");
            }
        }

        // GameItemDefinition helper methods
        internal static List<CategoryDefinition> GetGameItemDefinitionCategories(GameItemDefinition itemInstance)
        {
            if (itemInstance == null)
            {
                return null;
            }

            List<CategoryDefinition> categories = new List<CategoryDefinition>();
            itemInstance.GetCategories(categories);
            return categories;
        }

        // InventoryCatalog helper methods
        internal static List<InventoryItemDefinition> GetInventoryCatalogItemDefinitionsList()
        {
            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                return null;
            }

            List<InventoryItemDefinition> inventoryItemDefinitions = new List<InventoryItemDefinition>();
            GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions(inventoryItemDefinitions);
            return inventoryItemDefinitions;
        }

        internal static List<CategoryDefinition> GetInventoryCatalogCategoriesList()
        {
            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                return null;
            }

            List<CategoryDefinition> categories = new List<CategoryDefinition>();
            GameFoundationSettings.database.inventoryCatalog.GetCategories(categories);
            return categories;
        }

        internal static List<InventoryDefinition> GetInventoryCatalogCollectionDefinitionsList()
        {
            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                return null;
            }

            List<InventoryDefinition> inventoryDefinitions = new List<InventoryDefinition>();
            GameFoundationSettings.database.inventoryCatalog.GetCollectionDefinitions(inventoryDefinitions);
            return inventoryDefinitions;
        }

        internal static void AddItemDefinitionToInventoryCatalog(InventoryItemDefinition item)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.AddItemDefinition(item);
            }
            else
            {
                Debug.LogError("Inventory Item " + item.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveItemDefinitionFromInventoryCatalog(InventoryItemDefinition item)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.database.inventoryCatalog.RemoveItemDefinition(item);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Inventory Item " + item.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Inventory Item " + item.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        internal static void AddCategoryDefinitionToInventoryCatalog(CategoryDefinition category)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.AddCategory(category);
            }
            else
            {
                Debug.LogError("Category " + category.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveCategoryDefinitionFromInventoryCatalog(CategoryDefinition category)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.database.inventoryCatalog.RemoveCategory(category);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Category " + category.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Category " + category.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        internal static void AddInventoryDefinitionToInventoryCatalog(InventoryDefinition inventory)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.AddCollectionDefinition(inventory);
            }
            else
            {
                Debug.LogError("Inventory " + inventory.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveInventoryDefinitionFromInventoryCatalog(InventoryDefinition inventory)
        {
            if (GameFoundationSettings.database.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.database.inventoryCatalog.RemoveCollectionDefinition(inventory);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Inventory " + inventory.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Inventory " + inventory.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        // Stat catalog helper methods
        internal static void AddStatDefinitionToStatCatalog(StatDefinition item)
        {
            if (GameFoundationSettings.database.statCatalog != null)
            {
                GameFoundationSettings.database.statCatalog.AddStatDefinition(item);
            }
            else
            {
                Debug.LogError("Stat definition " + item.displayName + " could not be added to the stat catalog because catalog is null");
            }
        }

        internal static void RemoveStatDefinitionFromStatCatalog(StatDefinition item)
        {
            if (GameFoundationSettings.database.statCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.database.statCatalog.RemoveStatDefinition(item);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Stat definition " + item.displayName + " was unable to be removed from stat catalog list.");
                }
            }
            else
            {
                Debug.LogError("Stat definition " + item.displayName + " could not be removed from stat catalog because catalog is null");
            }
        }

        internal static List<StatDefinition> GetStatCatalogDefinitionsList()
        {
            if (GameFoundationSettings.database.statCatalog == null)
            {
                return null;
            }

            List<StatDefinition> statDefinitions = new List<StatDefinition>();
            GameFoundationSettings.database.statCatalog.GetStatDefinitions(statDefinitions);
            return statDefinitions;
        }

        // InventoryItemDefinition helper methods
        internal static InventoryItemDefinition CreateInventoryItemDefinition(string id, string displayName)
        {
            return InventoryItemDefinition.Create(id, displayName);
        }

        // CategoryDefinition helper methods
        internal static CategoryDefinition CreateCategoryDefinition(string categoryId, string displayName)
        {
            return new CategoryDefinition(categoryId, displayName);
        }

        // InventoryDefinition helper methods
        internal static InventoryDefinition CreateInventoryDefinition(string id, string displayName)
        {
            return InventoryDefinition.Create(id, displayName);
        }

        // StatDefinition helper methods
        internal static StatDefinition CreateStatDefinition(string id, string displayName, StatDefinition.StatValueType statValueType)
        {
            return new StatDefinition(id, displayName, statValueType);
        }
    }
}
