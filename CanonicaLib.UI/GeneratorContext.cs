using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services;

namespace Zen.CanonicaLib.UI
{
    public class GeneratorContext
    {
        public Assembly Assembly { get; init; }

        public OpenApiDocument Document { get; init; }

        public DiscoveryService DiscoveryService { get; init; }

        public ILibrary Library { get; init; }

        public Dictionary<string, IOpenApiSchema> Schemas { get; set; } = new Dictionary<string, IOpenApiSchema>();

        public GeneratorContext(Assembly assembly, DiscoveryService discoveryService)
        {
            Assembly = assembly;
            DiscoveryService = discoveryService;
            Library = DiscoveryService.GetLibraryInstance(assembly);
            var document = new OpenApiDocument();
            document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            Document = document;
        }
    }
}
