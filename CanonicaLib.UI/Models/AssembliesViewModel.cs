using System.ComponentModel.DataAnnotations;

namespace Zen.CanonicaLib.UI.Models
{
    /// <summary>
    /// View model for displaying assemblies and their OpenAPI documentation.
    /// </summary>
    public sealed class AssembliesViewModel
    {
        /// <summary>
        /// Gets or sets the page title displayed in the UI.
        /// </summary>
        /// <value>The title shown on the assemblies overview page.</value>
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string PageTitle { get; set; } = "CanonicaLib Assemblies";

        /// <summary>
        /// Gets or sets the list of discovered assemblies.
        /// </summary>
        /// <value>A collection of assembly information for display.</value>
        public IList<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();

        /// <summary>
        /// Gets or sets the CanonicaLib configuration options.
        /// </summary>
        /// <value>The options used to configure the CanonicaLib UI.</value>
        [Required]
        public CanonicaLibOptions Options { get; set; } = new();
    }

    /// <summary>
    /// Represents information about a discovered assembly.
    /// </summary>
    public sealed class AssemblyInfo
    {
        /// <summary>
        /// Gets or sets the display name of the assembly.
        /// </summary>
        /// <value>The friendly name shown in the UI.</value>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL slug for the assembly.
        /// </summary>
        /// <value>The URL-safe identifier used in routing.</value>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
        public string Slug { get; set; } = string.Empty;
    }
}