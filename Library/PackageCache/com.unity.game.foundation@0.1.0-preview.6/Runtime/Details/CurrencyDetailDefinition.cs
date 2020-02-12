namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CurrencyDetailDefinition.  Attach to a GameItemDefinition to store the currency information.
    /// </summary>
    /// <inheritdoc/>
    public class CurrencyDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this CurrencyDetailDefinition.</returns>
        public override string DisplayName() { return "Currency Detail"; }

        /// <summary>
        /// Enum for currency type.  Sets to soft, hard, special, event or other types of currency.
        /// </summary>
        public enum CurrencyType
        {
            Soft,
            Hard,
            Special,
            Event,
            Other,
        }

        [SerializeField]
        CurrencyType m_CurrencyType;

        /// <summary>
        /// Currency type for this CurrencyDetailDefinition.  Soft, hard, special, event or other types of currency.
        /// </summary>
        /// <returns>Currency type for this CurrencyDetailDefinition.</returns>
        public CurrencyType currencyType
        {
            get { return m_CurrencyType; }
            internal set { m_CurrencyType = value; }
        }
    }
}
