using System.Reflection;
using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
  public interface ISecurityGenerator
  {
    IList<OpenApiSecurityRequirement> GenerateOperationSecurityRequirements(MethodInfo endpointDefinition, GeneratorContext generatorContext);
    IDictionary<string, IOpenApiSecurityScheme> GenerateSecuritySchemes(GeneratorContext generatorContext);

    /// <summary>
    /// Generates the document-level (global) security requirements. Defaults to none.
    /// </summary>
    IList<OpenApiSecurityRequirement> GenerateDocumentSecurityRequirements(GeneratorContext generatorContext)
      => new List<OpenApiSecurityRequirement>();

    /// <summary>
    /// Validates that every operation's security requirements reference a declared security scheme
    /// and that every requested scope is defined by that scheme. Returns a list of human-readable
    /// warning messages (empty when the security configuration is consistent). Defaults to no checks.
    /// </summary>
    IList<string> ValidateSecurity(GeneratorContext generatorContext)
      => new List<string>();
  }
}
