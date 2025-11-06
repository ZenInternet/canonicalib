using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultSchemasGenerator : ISchemasGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ISchemaGenerator _schemaGenerator;

        public DefaultSchemasGenerator(IDiscoveryService discoveryService, ISchemaGenerator schemaGenerator)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
        }

        public void GenerateSchemas(GeneratorContext generatorContext)
        {
            var types = _discoveryService.FindSchemaDefinitions(generatorContext.Assembly);

            foreach (var type in types)
            {
                _schemaGenerator.GenerateSchema(type, generatorContext);
            }
        }
    }
}
