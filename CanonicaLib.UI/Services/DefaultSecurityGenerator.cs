using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultSecurityGenerator : ISecurityGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger<DefaultSecurityGenerator> _logger;

        public DefaultSecurityGenerator(
          IDiscoveryService discoveryService,
          ILogger<DefaultSecurityGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDictionary<string, IOpenApiSecurityScheme> GenerateSecuritySchemes(GeneratorContext generatorContext)
        {
            var secureService = _discoveryService.GetSecureServiceInstance(generatorContext.Assembly);

            return secureService?.SecuritySchemes ?? new Dictionary<string, IOpenApiSecurityScheme>();
        }

        public IList<OpenApiSecurityRequirement> GenerateOperationSecurityRequirements(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            var securityAttributes = endpointDefinition.GetCustomAttributes<OpenApiSecurityAttribute>();
            var securityRequirements = new List<OpenApiSecurityRequirement>();

            foreach (var attribute in securityAttributes)
            {
                // The host document must be supplied so the reference resolves to the declared
                // scheme; without it the requirement serializes as an empty object ("{}").
                var schemeReference = new OpenApiSecuritySchemeReference(attribute.Scheme, generatorContext.Document);

                var requirement = new OpenApiSecurityRequirement()
                {
                    { schemeReference, attribute.Scopes.ToList() }
                };
                securityRequirements.Add(requirement);
            }

            return securityRequirements;
        }

        public IList<OpenApiSecurityRequirement> GenerateDocumentSecurityRequirements(GeneratorContext generatorContext)
        {
            var secureService = _discoveryService.GetSecureServiceInstance(generatorContext.Assembly);

            return secureService?.Security ?? new List<OpenApiSecurityRequirement>();
        }

        public IList<string> ValidateSecurity(GeneratorContext generatorContext)
        {
            var warnings = new List<string>();

            var declaredSchemes = _discoveryService
                .GetSecureServiceInstance(generatorContext.Assembly)?.SecuritySchemes
                ?? new Dictionary<string, IOpenApiSecurityScheme>();

            foreach (var controller in _discoveryService.FindControllerDefinitions(generatorContext.Assembly))
            {
                foreach (var endpoint in _discoveryService.FindEndpointDefinitions(controller))
                {
                    foreach (var attribute in endpoint.GetCustomAttributes<OpenApiSecurityAttribute>())
                    {
                        var operationName = $"{controller.Name}.{endpoint.Name}";

                        if (!declaredSchemes.TryGetValue(attribute.Scheme, out var scheme))
                        {
                            warnings.Add(
                                $"Operation '{operationName}' references security scheme '{attribute.Scheme}', " +
                                "which is not declared in the contract's ISecureService.SecuritySchemes.");
                            continue;
                        }

                        var definedScopes = GetDefinedScopes(scheme);
                        if (definedScopes == null)
                        {
                            // Non-OAuth2 scheme: scopes are not applicable, so nothing to cross-check.
                            continue;
                        }

                        foreach (var scope in attribute.Scopes)
                        {
                            if (!definedScopes.Contains(scope))
                            {
                                warnings.Add(
                                    $"Operation '{operationName}' requests scope '{scope}' which is not defined " +
                                    $"by any OAuth2 flow of scheme '{attribute.Scheme}'.");
                            }
                        }
                    }
                }
            }

            foreach (var warning in warnings)
            {
                _logger.LogWarning("Security validation: {Warning}", warning);
            }

            return warnings;
        }

        /// <summary>
        /// Returns the set of scope names defined across all OAuth2 flows of a scheme, or
        /// <c>null</c> when the scheme is not an OAuth2 scheme (scopes not applicable).
        /// </summary>
        private static ISet<string>? GetDefinedScopes(IOpenApiSecurityScheme scheme)
        {
            if (scheme is not OpenApiSecurityScheme concrete || concrete.Type != SecuritySchemeType.OAuth2)
            {
                return null;
            }

            var scopes = new HashSet<string>();
            var flows = concrete.Flows;
            if (flows == null)
            {
                return scopes;
            }

            foreach (var flow in new[] { flows.AuthorizationCode, flows.ClientCredentials, flows.Implicit, flows.Password })
            {
                if (flow?.Scopes != null)
                {
                    foreach (var scope in flow.Scopes.Keys)
                    {
                        scopes.Add(scope);
                    }
                }
            }

            return scopes;
        }
    }
}