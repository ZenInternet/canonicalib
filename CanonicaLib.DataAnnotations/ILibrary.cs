using Microsoft.OpenApi;
using System.Collections.Generic;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Defines the contract for canonical library metadata providers.
    /// Implementations should provide descriptive information about the library
    /// and its organization structure for documentation generation.
    /// </summary>
    public interface ILibrary
    {
        /// <summary>
        /// Gets the human-readable display name for this library.
        /// </summary>
        /// <value>A friendly name that will be displayed in documentation.</value>
        /// <example>
        /// <code>
        /// public string FriendlyName => "Zen > Contract > Users";
        /// </code>
        /// </example>
        string FriendlyName { get; }

        /// <summary>
        /// Gets the optional collection of tag groups for organizing OpenAPI documentation.
        /// Return <c>null</c> if no custom organization is needed.
        /// </summary>
        /// <value>A collection of tag groups, or <c>null</c> for default organization.</value>
        /// <example>
        /// <code>
        /// public IList&lt;OpenApiTagGroup&gt;? TagGroups => new List&lt;OpenApiTagGroup&gt;
        /// {
        ///     new OpenApiTagGroup("Schema Reference")
        ///     {
        ///         Tags = new List&lt;OpenApiTag&gt;
        ///         {
        ///             new OpenApiTag { Name = "Models", Description = "Data models for this capability." }
        ///         }
        ///     }
        /// };
        /// </code>
        /// </example>
        IList<OpenApiTagGroup>? TagGroups { get; }
    }
}