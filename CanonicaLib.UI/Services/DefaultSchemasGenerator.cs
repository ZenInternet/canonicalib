using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="ISchemasGenerator"/> that generates 
    /// OpenAPI schemas for all types discovered in an assembly.
    /// </summary>
    public sealed class DefaultSchemasGenerator : ISchemasGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly ILogger<DefaultSchemasGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSchemasGenerator"/> class.
        /// </summary>
        /// <param name="discoveryService">Service for discovering schema definitions.</param>
        /// <param name="schemaGenerator">Generator for individual schemas.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultSchemasGenerator(
            IDiscoveryService discoveryService, 
            ISchemaGenerator schemaGenerator,
            ILogger<DefaultSchemasGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates OpenAPI schemas for all schema definitions discovered in the assembly.
        /// </summary>
        /// <param name="generatorContext">The context containing the document and assembly information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generatorContext"/> is null.</exception>
        public IDictionary<string, IOpenApiSchema> GenerateSchemas(GeneratorContext generatorContext)
        {
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            var assembly = generatorContext.Assembly;
            _logger.LogDebug("Generating schemas for assembly: {AssemblyName}", assembly.FullName);

            try
            {
                var schemaDefinitions = _discoveryService.FindSchemaDefinitions(assembly);
                _logger.LogDebug("Found {SchemaCount} schema definitions", schemaDefinitions.Count);

                // First pass: create placeholder entries for all schemas to handle circular references
                CreateSchemaPlaceholders(schemaDefinitions, generatorContext);

                // Second pass: generate actual schema content
                GenerateSchemaContent(schemaDefinitions, generatorContext);

                return generatorContext.Schemas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate schemas for assembly: {AssemblyName}", assembly.FullName);
                throw;
            }
        }

        private void CreateSchemaPlaceholders(IList<Type> schemaDefinitions, GeneratorContext generatorContext)
        {
            _logger.LogDebug("Creating schema placeholders for {Count} definitions", schemaDefinitions.Count);

            foreach (var schemaDefinition in schemaDefinitions)
            {
                var schemaKey = GetSchemaKey(schemaDefinition);
                if (!generatorContext.Schemas.ContainsKey(schemaKey))
                {
                    // Create placeholder to handle circular references
                    generatorContext.Schemas.Add(schemaKey, new OpenApiSchema());
                    _logger.LogTrace("Created placeholder for schema: {SchemaKey}", schemaKey);
                }
            }
        }

        private void GenerateSchemaContent(IList<Type> schemaDefinitions, GeneratorContext generatorContext)
        {
            _logger.LogDebug("Generating schema content for {Count} definitions", schemaDefinitions.Count);

            var generatedCount = 0;
            var skippedCount = 0;

            foreach (var schemaDefinition in schemaDefinitions)
            {
                try
                {
                    _schemaGenerator.GenerateSchema(schemaDefinition, generatorContext);
                    generatedCount++;
                }
                catch (Exception ex)
                {
                    var schemaKey = GetSchemaKey(schemaDefinition);
                    _logger.LogWarning(ex, "Failed to generate schema for type: {SchemaKey}", schemaKey);
                    skippedCount++;
                }
            }

            _logger.LogDebug("Schema generation completed: {Generated} generated, {Skipped} skipped", 
                generatedCount, skippedCount);
        }

        private static string GetSchemaKey(Type type)
        {
            return type.FullName ?? type.Name;
        }
    }
}