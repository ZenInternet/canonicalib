using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IWebhooksGenerator"/> that generates 
    /// OpenAPI webhooks from webhook definitions.
    /// </summary>
    public sealed class DefaultWebhooksGenerator : IWebhooksGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly IOperationGenerator _operationGenerator;
        private readonly ILogger<DefaultWebhooksGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWebhooksGenerator"/> class.
        /// </summary>
        /// <param name="discoveryService">Service for discovering webhook definitions.</param>
        /// <param name="operationGenerator">Generator for OpenAPI operations.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultWebhooksGenerator(
            IDiscoveryService discoveryService, 
            IOperationGenerator operationGenerator,
            ILogger<DefaultWebhooksGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _operationGenerator = operationGenerator ?? throw new ArgumentNullException(nameof(operationGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates OpenAPI webhooks from the webhook definitions in the assembly.
        /// </summary>
        /// <param name="generatorContext">The context containing the document and assembly information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generatorContext"/> is null.</exception>
        public IDictionary<string, IOpenApiPathItem>? GenerateWebhooks(GeneratorContext generatorContext)
        {
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            var assembly = generatorContext.Assembly;
            _logger.LogDebug("Generating webhooks for assembly: {AssemblyName}", assembly.FullName);

            try
            {
                var webhooks = new Dictionary<string, IOpenApiPathItem>();
                var webhookDefinitions = _discoveryService.FindWebhookDefinitions(assembly);

                _logger.LogDebug("Found {WebhookCount} webhook definitions", webhookDefinitions.Count());

                foreach (var webhookDefinition in webhookDefinitions)
                {
                    ProcessWebhookDefinition(webhookDefinition, webhooks, generatorContext);
                }

                if (webhooks.Count > 0)
                {
                    return webhooks;
                }
                 
                return null;
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate webhooks for assembly: {AssemblyName}", assembly.FullName);
                throw;
            }
        }

        private void ProcessWebhookDefinition(Type webhookDefinition, IDictionary<string, IOpenApiPathItem> webhooks, GeneratorContext generatorContext)
        {
            var webhookAttribute = webhookDefinition.GetCustomAttribute<OpenApiWebhookAttribute>();
            if (webhookAttribute == null)
            {
                _logger.LogWarning("Webhook definition {WebhookName} missing OpenApiWebhookAttribute", webhookDefinition.Name);
                return;
            }

            var endpointDefinitions = _discoveryService.FindEndpointDefinitions(webhookDefinition);
            _logger.LogDebug("Processing webhook {WebhookPurpose} with {EndpointCount} endpoints", 
                webhookAttribute.Purpose, endpointDefinitions.Count());

            if (!webhooks.ContainsKey(webhookAttribute.Purpose))
            {
                webhooks[webhookAttribute.Purpose] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>()
                };
            }

            var webhookItem = webhooks[webhookAttribute.Purpose];

            foreach (var endpointDefinition in endpointDefinitions)
            {
                ProcessWebhookEndpoint(endpointDefinition, webhookItem, generatorContext, webhookAttribute.Purpose);
            }
        }

        private void ProcessWebhookEndpoint(
            MethodInfo endpointDefinition, 
            IOpenApiPathItem webhookItem, 
            GeneratorContext generatorContext,
            string webhookPurpose)
        {
            var endpointAttribute = endpointDefinition.GetCustomAttribute<OpenApiEndpointAttribute>();
            if (endpointAttribute == null)
            {
                _logger.LogWarning("Webhook endpoint {EndpointName} missing OpenApiEndpointAttribute", endpointDefinition.Name);
                return;
            }

            var method = HttpMethod.Parse(endpointAttribute.HttpMethod);
            if (method == null)
            {
                _logger.LogWarning("Invalid HTTP method '{HttpMethod}' for webhook endpoint {EndpointName}", 
                    endpointAttribute.HttpMethod, endpointDefinition.Name);
                return;
            }

            if (webhookItem.Operations!.ContainsKey(method))
            {
                _logger.LogWarning("Duplicate webhook operation {HttpMethod} for purpose {Purpose}", method, webhookPurpose);
                return;
            }

            try
            {
                var operation = _operationGenerator.GenerateOperation(endpointDefinition, generatorContext);
                webhookItem.Operations.Add(method, operation);
                _logger.LogDebug("Added webhook operation {HttpMethod} for purpose {Purpose}", method, webhookPurpose);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate webhook operation {HttpMethod} for purpose {Purpose}", method, webhookPurpose);
            }
        }
    }
}