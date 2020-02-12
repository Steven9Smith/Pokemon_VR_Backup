namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// AnalyticsDetailDefinition. Attach to a game item to have it automatically get tracked with analytics.
    /// </summary>
    /// <inheritdoc/>
    public class AnalyticsDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this AnalyticsDetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this AnalyticsDetailDefinition.</returns>
        public override string DisplayName() { return "Analytics Detail"; }
    }
}
