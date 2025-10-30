using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultPathsGenerator : IPathsGenerator
    {
        private readonly IDiscoveryService DiscoveryService;
        private readonly IOperationGenerator OperationGenerator;

        public DefaultPathsGenerator(IDiscoveryService discoveryService, IOperationGenerator operationGenerator)
        {
            DiscoveryService = discoveryService;
            OperationGenerator = operationGenerator;
        }

        public void GeneratePaths(GeneratorContext generatorContext)
        {
            var assembly = generatorContext.Assembly;

            var paths = new OpenApiPaths();

            var controllerDefinitions = DiscoveryService.FindControllerDefinitions(assembly);

            foreach (var controllerDefinition in controllerDefinitions)
            {
                var pathAttribute = controllerDefinition.GetCustomAttribute<OpenApiPathAttribute>();

                var endpointDefinitions = DiscoveryService.FindEndpointDefinitions(controllerDefinition);

                foreach (var endpointDefinition in endpointDefinitions)
                {
                    var endpointAttribute = endpointDefinition.GetCustomAttribute<OpenApiEndpointAttribute>();

                    if (endpointAttribute == null)
                        continue;

                    var fullPath = $"{pathAttribute!.PathPattern}/{endpointAttribute!.PathPattern}".Replace("//", "/");

                    if (!paths.ContainsKey(fullPath))
                        paths[fullPath] = new OpenApiPathItem()
                        {
                            Operations = new Dictionary<HttpMethod, OpenApiOperation>()
                        };

                    var path = paths[fullPath];

                    var method = HttpMethod.Parse(endpointAttribute!.HttpMethod);

                    if (path.Operations!.ContainsKey(method))
                        continue;

                    path.Operations.Add(method, OperationGenerator.GenerateOperation(endpointDefinition, generatorContext));
                }
            }

            generatorContext.Document.Paths = paths;
        }
    }
}