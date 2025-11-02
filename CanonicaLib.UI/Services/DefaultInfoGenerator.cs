using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultInfoGenerator : IInfoGenerator
    {
        private readonly IDiscoveryService DiscoveryService;

        public DefaultInfoGenerator(IDiscoveryService discoveryService)
        {
            DiscoveryService = discoveryService;
        }

        public OpenApiInfo GenerateInfo(GeneratorContext generatorContext)
        {
            var assembly = generatorContext.Assembly;
            var library = DiscoveryService.GetLibraryInstance(assembly);
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            if (DiscoveryService.HasIndexDocument(assembly))
            {
                description += "\n\n" + DiscoveryService.GetDocumentContent(assembly, "Index.md");
            }

            return new OpenApiInfo
            {
                Title = library.FriendlyName,
                Version = assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Description = description,
            };
        }
    }
}
