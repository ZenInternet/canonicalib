using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultWebhooksGenerator : IWebhooksGenerator
    {
        private readonly IDiscoveryService DiscoveryService;
        private readonly IOperationGenerator OperationGenerator;

        public DefaultWebhooksGenerator(IDiscoveryService discoveryService, IOperationGenerator operationGenerator)
        {
            DiscoveryService = discoveryService;
            OperationGenerator = operationGenerator;
        }

        public void GenerateWebhooks(GeneratorContext generatorContext)
        {
            var assembly = generatorContext.Assembly;

            Dictionary<string, IOpenApiPathItem> webhooks = new Dictionary<string, IOpenApiPathItem>();

            var webhookDefinitions = DiscoveryService.FindWebhookDefinitions(assembly);

            foreach (var webhookDefinition in webhookDefinitions)
            {
                var webhookAttribute = webhookDefinition.GetCustomAttribute<OpenApiWebhookAttribute>();
                if (webhookAttribute == null)
                    continue;

                var endpointDefinitions = DiscoveryService.FindEndpointDefinitions(webhookDefinition);

                if (!webhooks.ContainsKey(webhookAttribute.Purpose))
                {
                    webhooks[webhookAttribute.Purpose] = new OpenApiPathItem()
                    {
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>()
                    };
                }

                var webhookItem = webhooks[webhookAttribute.Purpose];

                foreach (var endpointDefinition in endpointDefinitions)
                {
                    var endpointAttribute = endpointDefinition.GetCustomAttribute<OpenApiEndpointAttribute>();

                    if (endpointAttribute == null)
                        continue;

                    var method = HttpMethod.Parse(endpointAttribute!.HttpMethod);

                    if (webhookItem.Operations!.ContainsKey(method))
                        continue;

                    webhookItem.Operations.Add(method, OperationGenerator.GenerateOperation(endpointDefinition, generatorContext));
                }

            }

            generatorContext.Document.Webhooks = webhooks;
        }
    }
}