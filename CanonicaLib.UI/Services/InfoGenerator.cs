using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class InfoGenerator
    {
        private readonly DiscoveryService DiscoveryService;

        public InfoGenerator(DiscoveryService discoveryService)
        {
            DiscoveryService = discoveryService;
        }

        public void GenerateInfo(GeneratorContext generatorContext)
        {
            var library = DiscoveryService.GetLibraryInstance(generatorContext.Assembly);
            var assembly = generatorContext.Assembly;
            var info = new OpenApiInfo
            {
                Title = library.FriendlyName,
                Version = assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "No description available.",
            };
            generatorContext.Document.Info = info;
        }
    }
}
