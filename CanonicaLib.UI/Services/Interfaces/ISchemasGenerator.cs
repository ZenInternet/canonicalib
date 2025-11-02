using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface ISchemasGenerator
    {
        IDictionary<string, IOpenApiSchema> GenerateSchemas(GeneratorContext generatorContext);
    }
}