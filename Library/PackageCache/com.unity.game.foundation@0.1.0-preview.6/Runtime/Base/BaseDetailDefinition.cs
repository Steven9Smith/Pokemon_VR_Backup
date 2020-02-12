namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// BaseDetailDefinition are used to modify CollectionDefinitions or 
    /// ItemDefinitions with constant values.  They may or may not create 
    /// runtime versions of themselves (i.e. BaseDetail) based on the 
    /// need for non-constant values.  
    /// </summary>
    public abstract class BaseDetailDefinition : ScriptableObject
    {
        // pointer to BaseItemDefinition OR BaseCollectionDefinition
        [SerializeField]
        private GameItemDefinition m_Owner;

        /// <summary>
        /// The GameItemDefinition this DetailDefinition is attached to. Can be cast to either a BaseItemDefinition
        /// or BaseCollectionDefinition.
        /// </summary>
        /// <returns>The GameItemDefinition this DetailDefinition is attached to.</returns>
        public GameItemDefinition owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }

        /// <summary>
        /// Returns 'friendly' display name for this DetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this DetailDefinition.</returns>
        public abstract string DisplayName();

        // build runtime (instance) version of this DetailDefinition
        // NOTE: this is not abstract because it's perfectly fine for a DetailDefinition NOT …
        //   …    to have a runtime version of itself

        /// <summary>
        /// Method to create specific type of runtime Detail, if needed.
        /// Note: don't override this method if the DetailDefinition is static and you do not need to
        /// allow GameItem to modify its data independently at runtime.
        /// </summary>
        /// <param name="newOwner">GameItem that owns the Detail that will be created (if any).</param>
        /// <returns>The runtime Detail instance (if needed) or null if no runtime Detail is required for this DetailDefinition.</returns>
        public virtual BaseDetail CreateDetail(GameItem newOwner)
        {
            return null;
        }
    }
}
