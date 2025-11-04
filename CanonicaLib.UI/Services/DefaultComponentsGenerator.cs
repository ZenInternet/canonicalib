using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IComponentsGenerator"/> that generates 
    /// the components section of OpenAPI documents.
    /// </summary>
    public sealed class DefaultComponentsGenerator : IComponentsGenerator
    {
        private readonly ISchemasGenerator _schemasGenerator;
        private readonly ISecurityGenerator _securityGenerator;
        private readonly ILogger<DefaultComponentsGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultComponentsGenerator"/> class.
        /// </summary>
        /// <param name="schemasGenerator">The generator for OpenAPI schemas.</param>
        /// <param name="securityGenerator">The generator for OpenAPI security schemes.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultComponentsGenerator(
            ISchemasGenerator schemasGenerator,
            ISecurityGenerator securityGenerator,
            ILogger<DefaultComponentsGenerator> logger)
        {
            _schemasGenerator = schemasGenerator ?? throw new ArgumentNullException(nameof(schemasGenerator));
            _securityGenerator = securityGenerator ?? throw new ArgumentNullException(nameof(securityGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates the components section for the OpenAPI document.
        /// </summary>
        /// <param name="generatorContext">The context containing the document and assembly information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generatorContext"/> is null.</exception>
        public OpenApiComponents? GenerateComponents(GeneratorContext generatorContext)
        {
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            _logger.LogDebug("Generating components section for assembly: {AssemblyName}",
                generatorContext.Assembly.FullName);

            try
            {
                return new OpenApiComponents()
                {
                    Schemas = _schemasGenerator.GenerateSchemas(generatorContext),
                    SecuritySchemes = _securityGenerator.GenerateSecuritySchemes(generatorContext)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate components section for assembly: {AssemblyName}",
                    generatorContext.Assembly.FullName);
                throw;
            }
        }
    }
}
