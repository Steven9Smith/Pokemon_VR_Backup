namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes preset values and rules for an Inventory. During runtime, it may
    /// be useful to refer back to the InventoryDefinition for the presets and rules,
    /// but the values cannot be changed at runtime.  The InventoryDefinition is
    /// also responsible for creating Inventories based on preset properties.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryDefinition : BaseCollectionDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// This creates a new InventoryDefinition.
        /// </summary>
        /// <param name="id">The Id of this InventoryDefinition.</param>
        /// <param name="displayName">The name this InventoryDefinition will have.</param>
        /// <returns>Reference to the InventoryDefinition that was created.</returns>
        public new static InventoryDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot create an InventoryDefinition in play mode.");

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("InventoryDefinition can only be alphanumeric with optional dashes or underscores.");
            }

            var inventoryDefinition = ScriptableObject.CreateInstance<InventoryDefinition>();
            inventoryDefinition.Initialize(id, displayName);
            inventoryDefinition.name = $"{id}_Inventory";

            return inventoryDefinition;
        }

        internal override Inventory CreateCollection(string collectionId, string displayName, int gameItemId = 0)
        {
            return new Inventory(this, collectionId, gameItemId);
        }

        /// <summary>
        /// Adds the given default item to this InventoryDefinition. 
        /// Note: this thows if item without a CurrencyDetailDefinition is added to the wallet.
        /// </summary>
        /// <param name="itemDefinition">The default InventoryItemDefinition to add.</param>
        /// <param name="quantity">Quantity of items to add (defaults to 0).</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public override bool AddDefaultItem(InventoryItemDefinition itemDefinition, int quantity = 0)
        {
            if (!ProtectWalletInventory(itemDefinition))
            {
                return false;
            }

            return base.AddDefaultItem(itemDefinition, quantity);
        }

        /// <summary>
        /// Adds the given default item to this InventoryDefinition. 
        /// Note: this thows if item without a CurrencyDetailDefinition is added to the wallet.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <returns>Whether or not the DefaultItem was added successfully.</returns>
        public override bool AddDefaultItem(DefaultItem defaultItem)
        {
            InventoryItemDefinition defaultItemDefinition =
                GameFoundationSettings.database.inventoryCatalog.GetItemDefinition(defaultItem.definitionHash);

            if (!ProtectWalletInventory(defaultItemDefinition))
            {
                return false;
            }

            return base.AddDefaultItem(defaultItem);
        }

        protected bool ProtectWalletInventory(InventoryItemDefinition itemDefinition)
        {
            if (hash == InventoryManager.walletInventoryHash)
            {
                if (itemDefinition == null)
                {
                    Debug.LogError("Invalid InventoryItemDefinition passed for default item to add to the wallet Inventory.");
                    return false;
                }

                if (itemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    Debug.LogError("It is not possible to add an item to the wallet that does NOT have a CurrencyDetailDefinition attached.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a summary string for this InventoryDefinition.
        /// </summary>
        /// <returns>Summary string for this InventoryDefinition.</returns>
        public override string ToString()
        {
            return $"InventoryDefinition(Id: '{id}' DisplayName: '{displayName}'";
        }
    }
}
