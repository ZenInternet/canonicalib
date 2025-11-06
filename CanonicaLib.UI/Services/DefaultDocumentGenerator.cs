using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentGenerator"/> that orchestrates 
    /// the generation of OpenAPI documents from .NET assemblies.
    /// </summary>
    public sealed class DefaultDocumentGenerator : IDocumentGenerator
    {
        private readonly IInfoGenerator _infoGenerator;
        private readonly ISchemasGenerator _schemasGenerator;
        private readonly IPathsGenerator _pathsGenerator;
        private readonly ISecurityGenerator _securityGenerator;
        private readonly ITagGroupsGenerator _tagGroupsGenerator;
        private readonly IWebhooksGenerator _webhooksGenerator;
        private readonly IServersGenerator _serversGenerator;
        private readonly ILogger<DefaultDocumentGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDocumentGenerator"/> class.
        /// </summary>
        /// <param name="infoGenerator">Generator for OpenAPI info section.</param>
        /// <param name="schemasGenerator">Generator for OpenAPI schemas section.</param>
        /// <param name="serversGenerator">Generator for OpenAPI servers section.</param>
        /// <param name="pathsGenerator">Generator for OpenAPI paths section.</param>
        /// <param name="securityGenerator">Generator for OpenAPI security section.</param>
        /// <param name="tagGroupsGenerator">Generator for OpenAPI tag groups extension.</param>
        /// <param name="webhooksGenerator">Generator for OpenAPI webhooks section.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultDocumentGenerator(
            IInfoGenerator infoGenerator,
            ISchemasGenerator schemasGenerator,
            IServersGenerator serversGenerator,
            IPathsGenerator pathsGenerator,
            ISecurityGenerator securityGenerator,
            ITagGroupsGenerator tagGroupsGenerator,
            IWebhooksGenerator webhooksGenerator,
            ILogger<DefaultDocumentGenerator> logger)
        {
            _infoGenerator = infoGenerator ?? throw new ArgumentNullException(nameof(infoGenerator));
            _schemasGenerator = schemasGenerator ?? throw new ArgumentNullException(nameof(schemasGenerator));
            _serversGenerator = serversGenerator ?? throw new ArgumentNullException(nameof(serversGenerator));
            _pathsGenerator = pathsGenerator ?? throw new ArgumentNullException(nameof(pathsGenerator));
            _securityGenerator = securityGenerator ?? throw new ArgumentNullException(nameof(securityGenerator));
            _tagGroupsGenerator = tagGroupsGenerator ?? throw new ArgumentNullException(nameof(tagGroupsGenerator));
            _webhooksGenerator = webhooksGenerator ?? throw new ArgumentNullException(nameof(webhooksGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an OpenAPI document from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to generate documentation for.</param>
        /// <returns>A complete OpenAPI document.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when document generation fails.</exception>
        public GeneratorContext GenerateDocument(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            _logger.LogInformation("Starting OpenAPI document generation for assembly: {AssemblyName}", assembly.FullName);

            try
            {
                var generatorContext = new GeneratorContext(assembly);

                // Generate document sections in the correct order
                _logger.LogDebug("Generating info section");
                generatorContext.Document.Info = _infoGenerator.GenerateInfo(generatorContext);

                _schemasGenerator.GenerateSchemas(generatorContext);

                _logger.LogDebug("Generating servers section");
                generatorContext.Document.Servers = _serversGenerator.GenerateServers(generatorContext);

                generatorContext.Document.Components!.SecuritySchemes = _securityGenerator.GenerateSecuritySchemes(generatorContext);

                _logger.LogDebug("Generating tag groups");
                generatorContext.Document.Tags = _tagGroupsGenerator.GenerateTags(generatorContext);
                var tagGroups = _tagGroupsGenerator.GenerateTagGroups(generatorContext);
                if (tagGroups != null)
                {
                    generatorContext.Document.Extensions!.Add("x-tagGroups", tagGroups);
                }

                _logger.LogDebug("Generating paths section");
                generatorContext.Document.Paths = _pathsGenerator.GeneratePaths(generatorContext);

                _logger.LogDebug("Generating webhooks section");
                generatorContext.Document.Webhooks = _webhooksGenerator.GenerateWebhooks(generatorContext);

                _logger.LogInformation("Successfully generated OpenAPI document for assembly: {AssemblyName}", assembly.FullName);

                return generatorContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate OpenAPI document for assembly: {AssemblyName}", assembly.FullName);
                throw new InvalidOperationException($"Failed to generate OpenAPI document for assembly '{assembly.FullName}'", ex);
            }
        }
    }
}
