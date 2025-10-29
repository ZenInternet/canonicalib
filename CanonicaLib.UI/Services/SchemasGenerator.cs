using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services
{
    public class SchemasGenerator
    {
        private readonly DiscoveryService DiscoveryService;

        private readonly SchemaGenerator SchemaGenerator;

        public SchemasGenerator(DiscoveryService discoveryService, SchemaGenerator schemaGenerator)
        {
            DiscoveryService = discoveryService;
            SchemaGenerator = schemaGenerator;
        }

        public void GenerateSchemas(GeneratorContext generatorContext)
        {
            var assembly = generatorContext.Assembly;
            var schemaDefinitions = DiscoveryService.FindSchemaDefinitions(assembly);

            foreach (var schemaDefinition in schemaDefinitions)
            {
                generatorContext.Schemas.Add(schemaDefinition.FullName, new OpenApiSchema());
            }

            //  for each schema definition, generate an OpenApiSchema and add it to the dictionary
            foreach (var schemaDefinition in schemaDefinitions)
            {
                SchemaGenerator.GenerateSchema(schemaDefinition, generatorContext, out var _);
            }
        }

    }
}