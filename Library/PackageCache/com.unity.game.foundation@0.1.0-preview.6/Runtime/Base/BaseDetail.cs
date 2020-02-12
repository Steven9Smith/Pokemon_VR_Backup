namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Details are used to track runtime modifications to a Collections and Items.  
    /// If desired, helper methods can be added to a individual Detail, as needed.
    /// Important note: Details must ALWAYS be constructed using DetailDefinitions 
    /// which is the same pattern used for all ‘runtime’ versions of other Definitions as well.
    /// </summary>
    public abstract class BaseDetail     //TODO API: (also see todo below) consider <T> for basebase or baseitem or basedetail   OR  use BaseItemDetail : BaseDetail  and  BaseCollectionDetail : BaseDetail so you can derive from either of THREE base classes
    {
        // pointer to GameItem that owns this Detail
        protected GameItem m_Owner;

        /// <summary>
        /// The GameItem that this Detail is attached to. May be castable to either a BaseItem or BaseCollection.
        /// </summary>
        /// <returns>The GameItem that this Detail is attached to.</returns>
        public GameItem owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }        //TODO API: even if neither above tech works, you COULD POSSIBLY use this so dev can say detail.ownerItem  VS   detail.ownerCollection and let us do the cast for him
	
        // definition used to create this Detail (or null)           
        private BaseDetailDefinition m_Definition;

        /// <summary>
        /// Retrieve a reference to the DetailDefinition used to make this Detail.
        /// </summary>
        /// <returns>DetailDefinition associated with this Detail.</returns>
        public BaseDetailDefinition definition
        {
            get { return m_Definition; } 
        }

        /// <summary>
        /// Creates BaseDetail with information about its owner and DetailDefinition it was made from. 
        /// Use DetailDefinition.CreateDetail() if you need to create a Detail.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="def"></param>
        protected BaseDetail(GameItem owner, BaseDetailDefinition def)
        {
            m_Definition = def;
            m_Owner = owner;
        }
    }
}
