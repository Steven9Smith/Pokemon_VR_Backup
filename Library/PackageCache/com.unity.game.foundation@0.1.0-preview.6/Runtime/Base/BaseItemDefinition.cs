using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The abstract ItemDefinition used to create Items which stores
    /// all needed constant data for an Item.  ItemDefinitions should
    /// only exist in CollectionDefinitions.
    /// </summary>
    /// <typeparam name="T1">The type of CollectinDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this BaseItemDefinition uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseItemDefinition<T1, T2, T3, T4> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {        
        /// <summary>
        /// This will spawn a new Item based off of this ItemDefinition.
        /// </summary>
        /// <returns>Reference to the newly created Item.</returns>
        internal abstract T4 CreateItem(BaseCollection<T1, T2, T3, T4> owner, int gameItemId = 0);
    }
}
