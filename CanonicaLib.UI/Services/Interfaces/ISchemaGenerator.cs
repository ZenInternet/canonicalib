using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface ISchemaGenerator
    {
        IOpenApiSchema? GenerateSchema(Type schemaDefinition, GeneratorContext generatorContext);
    }
}