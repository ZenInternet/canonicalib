using Zen.CanonicaLib.DataAnnotations;
using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class PathsGenerator
    {
        private readonly DiscoveryService DiscoveryService;
        private readonly OperationGenerator OperationGenerator;

        public PathsGenerator(DiscoveryService discoveryService, OperationGenerator operationGenerator)
        {
            DiscoveryService = discoveryService;
            OperationGenerator = operationGenerator;
        }
        public OpenApiPaths GeneratePaths(Assembly assembly)
        {
            var paths = new OpenApiPaths();

            var controllerDefinitions = DiscoveryService.FindControllerDefinitions(assembly);

            foreach (var controllerDefinition in controllerDefinitions)
            {
                var pathAttribute = controllerDefinition.GetCustomAttribute<PathAttribute>();

                var endpointDefinitions = DiscoveryService.FindEndpointDefinitions(controllerDefinition);

                foreach (var endpointDefinition in endpointDefinitions)
                {
                    var endpointAttribute = endpointDefinition.GetCustomAttribute<EndpointAttribute>();

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

                    path.Operations.Add(method, OperationGenerator.GenerateOpration(endpointDefinition));
                }
            }

            return paths;
        }
    }
}