using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IPathsGenerator"/> that generates 
    /// OpenAPI paths from controller and endpoint definitions.
    /// </summary>
    public sealed class DefaultPathsGenerator : IPathsGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly IOperationGenerator _operationGenerator;
        private readonly ILogger<DefaultPathsGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPathsGenerator"/> class.
        /// </summary>
        /// <param name="discoveryService">Service for discovering controller and endpoint definitions.</param>
        /// <param name="operationGenerator">Generator for OpenAPI operations.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultPathsGenerator(
            IDiscoveryService discoveryService, 
            IOperationGenerator operationGenerator,
            ILogger<DefaultPathsGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _operationGenerator = operationGenerator ?? throw new ArgumentNullException(nameof(operationGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates OpenAPI paths from the controller and endpoint definitions in the assembly.
        /// </summary>
        /// <param name="generatorContext">The context containing the document and assembly information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generatorContext"/> is null.</exception>
        public OpenApiPaths GeneratePaths(GeneratorContext generatorContext)
        {
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            var assembly = generatorContext.Assembly;
            _logger.LogDebug("Generating paths for assembly: {AssemblyName}", assembly.FullName);

            try
            {
                var paths = new OpenApiPaths();
                var controllerDefinitions = _discoveryService.FindControllerDefinitions(assembly);

                _logger.LogDebug("Found {ControllerCount} controller definitions", controllerDefinitions.Count());

                foreach (var controllerDefinition in controllerDefinitions)
                {
                    ProcessControllerDefinition(controllerDefinition, paths, generatorContext);
                }

                return paths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate paths for assembly: {AssemblyName}", assembly.FullName);
                throw;
            }
        }

        private void ProcessControllerDefinition(Type controllerDefinition, OpenApiPaths paths, GeneratorContext generatorContext)
        {
            var pathAttribute = controllerDefinition.GetCustomAttribute<OpenApiPathAttribute>();
            var endpointDefinitions = _discoveryService.FindEndpointDefinitions(controllerDefinition);

            _logger.LogDebug("Processing controller {ControllerName} with {EndpointCount} endpoints", 
                controllerDefinition.Name, endpointDefinitions.Count());

            foreach (var endpointDefinition in endpointDefinitions)
            {
                ProcessEndpointDefinition(endpointDefinition, pathAttribute, paths, generatorContext);
            }
        }

        private void ProcessEndpointDefinition(
            MethodInfo endpointDefinition, 
            OpenApiPathAttribute? pathAttribute, 
            OpenApiPaths paths, 
            GeneratorContext generatorContext)
        {
            var endpointAttribute = endpointDefinition.GetCustomAttribute<OpenApiEndpointAttribute>();
            if (endpointAttribute == null)
            {
                _logger.LogWarning("Endpoint {EndpointName} missing OpenApiEndpointAttribute", endpointDefinition.Name);
                return;
            }

            var fullPath = BuildFullPath(pathAttribute?.PathPattern, endpointAttribute.PathPattern);
            
            if (!paths.ContainsKey(fullPath))
            {
                paths[fullPath] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>()
                };
            }

            var pathItem = paths[fullPath];
            
            var method = HttpMethod.Parse(endpointAttribute!.HttpMethod);
            if (method == null)
            {
                _logger.LogWarning("Invalid HTTP method '{HttpMethod}' for endpoint {EndpointName}",
                    endpointAttribute.HttpMethod, endpointDefinition.Name);
                return;
            }

            if (pathItem.Operations!.ContainsKey(method))
            {
                _logger.LogWarning("Duplicate operation {HttpMethod} {Path} found", method, fullPath);
                return;
            }

            try
            {
                var operation = _operationGenerator.GenerateOperation(endpointDefinition, generatorContext);
                pathItem.Operations.Add(method, operation);
                _logger.LogDebug("Added operation {HttpMethod} {Path}", method, fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate operation for {HttpMethod} {Path}", method, fullPath);
            }
        }

        private static string BuildFullPath(string? basePath, string? endpointPath)
        {
            if (string.IsNullOrWhiteSpace(basePath) && string.IsNullOrWhiteSpace(endpointPath))
                return "/";

            var combined = $"{basePath?.TrimEnd('/')}/{endpointPath?.TrimStart('/')}";
            return combined.Replace("//", "/").TrimEnd('/');
        }
    }
}