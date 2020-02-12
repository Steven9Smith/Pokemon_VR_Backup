namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Preset values and rules for an InventoryItem.
    /// During runtime, it may be useful to refer back to the InventoryItemDefinition for
    /// the presets and rules, but the values cannot be changed at runtime.
    /// InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItemDefinition : BaseItemDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        internal override InventoryItem CreateItem(BaseCollection<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem> owner, int gameItemId = 0)
        {
            return new InventoryItem(this, owner as Inventory, gameItemId);
        }

        /// <summary>
        /// Creates a new InventoryItemDefinition.
        /// </summary>
        /// <param name="id">The Id this InventoryItemDefinition will use.</param>
        /// <param name="displayName">The display name of the InventoryItemDefinition.</param>
        /// <returns>Reference to the newly made InventoryItemDefinition.</returns>
        public new static InventoryItemDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot make an InventoryItemDefinition while in play mode.");

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("InventoryItemDefinition id can only be alphanumeric with optional dashes or underscores.");
            }

            InventoryItemDefinition definition = ScriptableObject.CreateInstance<InventoryItemDefinition>();
            definition.Initialize(id, displayName);
            definition.name = $"{id}_InventoryItem";

            return definition;
        }

        protected override CategoryDefinition GetCategoryDefinition(int categoryHash)
        {
            return GameFoundationSettings.database.inventoryCatalog.GetCategory(categoryHash);
        }

        internal override void OnRemove()
        {
            if (Application.isPlaying)
            {
                throw new System.Exception("InventoryItemDefinitions cannot be removed during play mode.");
            }

            base.OnRemove();

            RemoveItemFromInventoriesDefaultItems(this);
        }

        private void RemoveItemFromInventoriesDefaultItems(InventoryItemDefinition item)
        {
            var inventoryCatalogAllInventoryDefinitions = GameFoundationSettings.database.inventoryCatalog.GetCollectionDefinitions();
            foreach (InventoryDefinition inventoryDefinition in inventoryCatalogAllInventoryDefinitions)
            {
                DefaultItem[] defaultItems = inventoryDefinition.GetDefaultItems();
                foreach (DefaultItem defaultItem in defaultItems)
                {
                    if (defaultItem.definitionHash == item.hash)
                    {
                        inventoryDefinition.RemoveDefaultItem(defaultItem);
                    }
                }
            }
        }
    }
}
