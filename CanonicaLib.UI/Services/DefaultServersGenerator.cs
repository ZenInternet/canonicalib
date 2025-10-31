using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultServersGenerator : IServersGenerator
    {
        IDiscoveryService DiscoveryService;

        public DefaultServersGenerator(IDiscoveryService discoveryService)
        {
            DiscoveryService = discoveryService;
        }

        public void GenerateServers(GeneratorContext generatorContext)
        {
            var service = DiscoveryService.GetServiceInstance(generatorContext.Assembly);

            if (service?.Servers != null && service.Servers.Any())
            {
                generatorContext.Document.Servers = service.Servers;
            }
        }
    }
}
