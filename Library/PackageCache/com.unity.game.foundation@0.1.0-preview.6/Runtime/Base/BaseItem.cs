using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Anything that goes into a Collection should be an Item, and
    /// Items should only exist in Collections.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this BaseItem uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this BaseItem uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this BaseItem uses.</typeparam>
    /// <typeparam name="T4">The type of Items this BaseItem uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseItem<T1, T2, T3, T4> : GameItem
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        protected BaseItem(T3 definition, T2 owner) : this(definition, owner, 0){}
        
        internal BaseItem(T3 definition, T2 owner, int gameItemId) : base(definition, definition.id, gameItemId)
        {
            m_Definition = definition;
            m_Owner = owner;
        }

        [SerializeField] 
        private T3 m_Definition;

        /// <summary>
        /// The ItemDefinition this Item is based on.
        /// </summary>
        /// <returns>The ItemDefinition this Item is based on.</returns>
        public new T3 definition
        {
            get { return m_Definition; }
        }

        /// <summary>
        /// The ItemDefinition Id (string) this Item is based on.
        /// </summary>
        /// <returns>ItemDefinition Id this Item is based on.</returns>
        public string definitionId
        {
            get { return m_Definition.id; }
        }

        private T2 m_Owner;

        /// <summary>
        /// The Collection that this Item belongs to.
        /// </summary>
        /// <returns>The Collection that this Item belongs to.</returns>
        protected T2 owner
        {
            get { return m_Owner; }
        }

        [SerializeField]
        private int m_intValue;

        /// <summary>
        /// The integer value associated with this Item, usually signifying quantity of Items in a Collection.
        /// </summary>
        /// <returns>The integer value associated with this Item.</returns>
        public int intValue
        {
            get { return m_intValue; }
            internal set { m_intValue = value; }
        }

        /// <summary>
        /// Gets the specified DetailDefinition attached to this Item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The specified DetailDefinition attached to this Item.</returns>
        public T GetDetailDefinition<T>() where T : BaseDetailDefinition
        {
            return m_Definition?.GetDetailDefinition<T>();
        }
    }
}
