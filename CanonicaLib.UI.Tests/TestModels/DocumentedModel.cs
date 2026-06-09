using System.ComponentModel;

namespace Zen.CanonicaLib.UI.Tests.TestModels
{
    /// <summary>
    /// Model used to verify that per-property descriptions are emitted into the generated schema.
    /// </summary>
    public class DocumentedModel
    {
        /// <summary>
        /// The unique account number.
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// The number of active lines.
        /// </summary>
        public int LineCount { get; set; }

        /// <summary>
        /// The roles held on the account.
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// The billing address for the account.
        /// </summary>
        public DocumentedAddress? BillingAddress { get; set; }

        [Description("The description-attribute fallback value.")]
        public string? FallbackOnly { get; set; }

        // Intentionally undocumented to verify no description is invented.
        public string? Undocumented { get; set; }
    }

    /// <summary>
    /// A postal address used by <see cref="DocumentedModel"/>.
    /// </summary>
    public class DocumentedAddress
    {
        /// <summary>
        /// The first line of the address.
        /// </summary>
        public string? Line1 { get; set; }
    }
}
