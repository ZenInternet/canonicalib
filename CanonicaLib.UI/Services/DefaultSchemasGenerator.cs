using Microsoft.Extensions.Logging;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultSchemasGenerator : ISchemasGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly ILogger<DefaultSchemasGenerator> _logger;

        public DefaultSchemasGenerator(IDiscoveryService discoveryService, ISchemaGenerator schemaGenerator, ILogger<DefaultSchemasGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void GenerateSchemas(GeneratorContext generatorContext)
        {
            var types = _discoveryService.FindSchemaDefinitions(generatorContext.Assembly);
            _logger.LogInformation("Discovered {TypeCount} schema definition types.", types.Count);

            foreach (var type in types)
            {
                _logger.LogInformation("Generating schema for discovered type: {TypeName}", type.FullName);
                _schemaGenerator.GenerateSchema(type, generatorContext);
            }
        }
    }
}
