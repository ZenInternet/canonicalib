using Microsoft.OpenApi;
using System.Collections.Generic;

namespace Zen.CanonicaLib.DataAnnotations
{
    public interface ISecureService
    {
        IDictionary<string, IOpenApiSecurityScheme>? SecuritySchemes { get; }
    }
}