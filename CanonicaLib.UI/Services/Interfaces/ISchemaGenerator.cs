using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface ISchemaGenerator
    {
        void GenerateSchema(Type schemaDefinition, GeneratorContext generatorContext, out IOpenApiSchema? openApiSchema);
    }
}