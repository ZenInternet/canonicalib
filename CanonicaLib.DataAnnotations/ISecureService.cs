using Microsoft.OpenApi;
using System.Collections.Generic;

namespace Zen.CanonicaLib.DataAnnotations
{
    public interface ISecureService
    {
        IDictionary<string, IOpenApiSecurityScheme>? SecuritySchemes { get; }

        /// <summary>
        /// Optional document-level (global) security requirements applied to every operation
        /// unless an operation overrides them with its own <c>OpenApiSecurityAttribute</c>.
        /// Return <c>null</c> (the default) to emit no root-level <c>security</c> and rely solely
        /// on per-operation requirements.
        /// </summary>
        IList<OpenApiSecurityRequirement>? Security => null;
    }
}