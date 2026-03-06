using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Specifies MCP (Model Context Protocol) tool metadata for an OpenAPI endpoint operation.
    /// Outputs a custom <c>x-mcp</c> extension on the operation in the generated OpenAPI document.
    /// The operation's existing <c>operationId</c> serves as the MCP tool name.
    /// </summary>
    /// <remarks>
    /// Properties mirror the MCP tool annotations from the
    /// <see href="https://modelcontextprotocol.io/specification">MCP specification</see>.
    /// </remarks>
    /// <example>
    /// <code>
    /// [OpenApiMcp(
    ///     Title = "Get Communication Preferences",
    ///     Description = "Retrieve the authenticated person's communication preferences across all categories.",
    ///     ReadOnly = true)]
    /// [OpenApiEndpoint("/preferences", Methods.MethodGet)]
    /// public PreferencesResponse GetPreferences();
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class OpenApiMcpAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the MCP tool description.
        /// This is distinct from the OpenAPI operation description and is used by AI models
        /// to understand when and how to use the tool.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a human-readable title for the tool that can be displayed to users.
        /// Unlike the tool name, the title can include spaces and natural language phrasing.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether this tool does not modify its environment.
        /// When <c>true</c>, the tool only performs read operations without changing state.
        /// Default is <c>false</c>.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the tool might perform destructive updates to its environment.
        /// Default is <c>true</c>.
        /// </summary>
        public bool Destructive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether calling the tool repeatedly with the same arguments
        /// has no additional effect on its environment.
        /// Default is <c>false</c>.
        /// </summary>
        public bool Idempotent { get; set; }

        /// <summary>
        /// Gets or sets whether this tool can interact with an "open world" of external entities
        /// (e.g. web search), as opposed to a closed, well-defined domain.
        /// Default is <c>true</c>.
        /// </summary>
        public bool OpenWorld { get; set; } = true;
    }
}
