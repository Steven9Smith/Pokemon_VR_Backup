namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// IconDetailDefinition.  Attach to a GameItemDefinition to store sprite information.
    /// </summary>
    /// <inheritdoc/>
    public class IconDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this IconDetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this IconDetailDefinition.</returns>
        public override string DisplayName() { return "Icon Detail"; }

        [SerializeField]
        private Sprite m_Icon;

        /// <summary>
        /// Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailDefinition is attached.
        /// </summary>
        /// <returns>Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailDefinition is attached.</returns>
        public Sprite icon
        {
            get { return m_Icon; }
            internal set { m_Icon = value; }
        }
    }
}
