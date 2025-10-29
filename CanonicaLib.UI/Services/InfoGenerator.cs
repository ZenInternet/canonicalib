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
            var assembly = generatorContext.Assembly;
            var library = DiscoveryService.GetLibraryInstance(assembly);
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            if (DiscoveryService.HasIndexDocument(assembly))
            {
                description += "\n\n" + DiscoveryService.GetDocumentContent(assembly, "Index.md");
            }

            var info = new OpenApiInfo
            {
                Title = library.FriendlyName,
                Version = assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Description = description, 
            };
            generatorContext.Document.Info = info;
        }
    }
}
