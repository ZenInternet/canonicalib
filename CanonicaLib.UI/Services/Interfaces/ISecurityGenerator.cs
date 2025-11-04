using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
  public interface ISecurityGenerator
  {
    IDictionary<string, IOpenApiSecurityScheme> GenerateSecuritySchemes(GeneratorContext generatorContext);
  }
}
