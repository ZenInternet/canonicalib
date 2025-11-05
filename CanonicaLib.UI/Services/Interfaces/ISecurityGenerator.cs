using System.Reflection;
using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
  public interface ISecurityGenerator
  {
    IList<OpenApiSecurityRequirement> GenerateOperationSecurityRequirements(MethodInfo endpointDefinition);
    IDictionary<string, IOpenApiSecurityScheme> GenerateSecuritySchemes(GeneratorContext generatorContext);
  }
}
