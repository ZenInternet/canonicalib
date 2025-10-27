using Microsoft.OpenApi;
using System.Reflection;

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

        internal IDictionary<string, IOpenApiSchema> GenerateSchemas(Assembly assembly)
        {
            var schemas = new Dictionary<string, IOpenApiSchema>();

            var schemaDefinitions = DiscoveryService.FindSchemaDefinitions(assembly);

            //  for each schema definition, generate an OpenApiSchema and add it to the dictionary
            foreach (var schemaDefinition in schemaDefinitions)
            {
                var schema = SchemaGenerator.GenerateSchema(schemaDefinition, schemaDefinition, assembly);
                    
                schemas.Add(schemaDefinition.FullName.Replace(".", "_"), schema);
            }

            return schemas;

        }

    }
}