using Microsoft.OpenApi;
using System.ComponentModel.DataAnnotations;

namespace Zen.CanonicaLib.UI
{
    /// <summary>
    /// Configuration options for CanonicaLib UI and API generation.
    /// </summary>
    /// <remarks>
    /// This record provides all the configuration settings needed to customize
    /// the behavior and appearance of the CanonicaLib documentation UI.
    /// </remarks>
    public sealed record CanonicaLibOptions
    {
        /// <summary>
        /// Gets or sets the page title displayed in the UI.
        /// </summary>
        /// <value>The title shown in the browser tab and page header. Defaults to "CanonicaLib Assemblies".</value>
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string PageTitle { get; set; } = "CanonicaLib Assemblies";

        /// <summary>
        /// Gets the root path where the CanonicaLib UI is hosted.
        /// </summary>
        /// <value>The base URL path for the CanonicaLib interface. Defaults to "/canonicalib".</value>
        /// <remarks>
        /// This path is used as the base for all CanonicaLib routes. It should start with a forward slash
        /// and not end with one (unless it's just "/").
        /// </remarks>
        [Required]
        [RegularExpression(@"^\/[a-zA-Z0-9\-_\/]*[^\/]$|^\/$", ErrorMessage = "Root path must start with '/' and not end with '/' (unless it's just '/')")]
        public string RootPath { get; init; } = "/canonicalib";

        /// <summary>
        /// Gets the API path relative to the root path where OpenAPI JSON specifications are served.
        /// </summary>
        /// <value>The path suffix for API endpoints. Defaults to "/api".</value>
        /// <remarks>
        /// This path is appended to the RootPath to create the full API endpoint URLs.
        /// For example, with RootPath="/canonicalib" and ApiPath="/api", the full API path becomes "/canonicalib/api".
        /// </remarks>
        [Required]
        [RegularExpression(@"^\/[a-zA-Z0-9\-_\/]*[^\/]$", ErrorMessage = "API path must start with '/' and not end with '/'")]
        public string ApiPath { get; init; } = "/api";

        /// <summary>
        /// Gets the root namespace prefix to remove from URLs and titles in the UI.
        /// </summary>
        /// <value>The namespace prefix to strip for cleaner display. Defaults to empty string.</value>
        /// <remarks>
        /// When set, this namespace prefix is removed from assembly names and titles to provide
        /// cleaner, more readable names in the documentation interface.
        /// </remarks>
        [StringLength(200)]
        public string RootNamespace { get; init; } = "";

        /// <summary>
        /// Gets the post-processing functions for customizing OpenAPI generation.
        /// </summary>
        /// <value>A collection of post-processors for customizing the generated OpenAPI specifications.</value>
        /// <remarks>
        /// These processors allow for custom modification of generated OpenAPI elements
        /// after the initial generation phase is complete.
        /// </remarks>
        public PostProcessors? PostProcessors { get; init; } = new();
    }

    /// <summary>
    /// Provides post-processing functions for customizing OpenAPI generation.
    /// </summary>
    /// <remarks>
    /// This record contains delegates that can be used to modify OpenAPI elements
    /// after they have been generated but before the final document is created.
    /// </remarks>
    public sealed record PostProcessors
    {
        /// <summary>
        /// Gets or sets a function to post-process OpenAPI parameters.
        /// </summary>
        /// <value>A function that transforms the list of parameters. Can be null if no processing is needed.</value>
        /// <remarks>
        /// This function is called after parameters are generated and allows for custom
        /// modification, filtering, or transformation of the parameter list.
        /// </remarks>
        public Func<IList<IOpenApiParameter>?, IList<IOpenApiParameter>?>? ParametersProcessor { get; init; }

        /// <summary>
        /// Gets or sets a function to post-process OpenAPI headers.
        /// </summary>
        /// <value>A function that transforms the dictionary of headers. Can be null if no processing is needed.</value>
        /// <remarks>
        /// This function is called after headers are generated and allows for custom
        /// modification, filtering, or transformation of the headers collection.
        /// </remarks>
        public Func<IDictionary<string, IOpenApiHeader>?, IDictionary<string, IOpenApiHeader>?>? HeadersProcessor { get; init; }
    }
}
