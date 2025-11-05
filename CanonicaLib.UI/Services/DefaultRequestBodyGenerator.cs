using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IRequestBodyGenerator"/> that generates 
    /// OpenAPI request bodies from method parameters.
    /// </summary>
    public sealed class DefaultRequestBodyGenerator : IRequestBodyGenerator
    {
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly IExamplesGenerator _examplesGenerator;
        private readonly ILogger<DefaultRequestBodyGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestBodyGenerator"/> class.
        /// </summary>
        /// <param name="schemaGenerator">Generator for OpenAPI schemas.</param>
        /// <param name="examplesGenerator">Generator for OpenAPI examples.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultRequestBodyGenerator(
            ISchemaGenerator schemaGenerator, 
            IExamplesGenerator examplesGenerator,
            ILogger<DefaultRequestBodyGenerator> logger)
        {
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            _examplesGenerator = examplesGenerator ?? throw new ArgumentNullException(nameof(examplesGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an OpenAPI request body from the method's parameters.
        /// </summary>
        /// <param name="endpointDefinition">The method definition to analyze for request body parameters.</param>
        /// <param name="generatorContext">The context containing schemas and assembly information.</param>
        /// <returns>An OpenAPI request body if a request body parameter is found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public IOpenApiRequestBody? GenerateRequestBody(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            if (endpointDefinition == null)
                throw new ArgumentNullException(nameof(endpointDefinition));
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            _logger.LogDebug("Generating request body for endpoint: {EndpointName}", endpointDefinition.Name);

            try
            {
                var requestBodyParameter = FindRequestBodyParameter(endpointDefinition);
                if (requestBodyParameter == null)
                {
                    _logger.LogDebug("No request body parameter found for endpoint: {EndpointName}", endpointDefinition.Name);
                    return null;
                }

                _logger.LogDebug("Found request body parameter: {ParameterName} of type: {ParameterType}", 
                    requestBodyParameter.Name, requestBodyParameter.ParameterType.FullName);

                var requestBody = CreateRequestBody(requestBodyParameter, generatorContext);
                
                _logger.LogDebug("Successfully generated request body for endpoint: {EndpointName}", endpointDefinition.Name);
                return requestBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate request body for endpoint: {EndpointName}", endpointDefinition.Name);
                throw new InvalidOperationException($"Failed to generate request body for endpoint '{endpointDefinition.Name}'", ex);
            }
        }

        private static ParameterInfo? FindRequestBodyParameter(MethodInfo endpointDefinition)
        {
            return endpointDefinition.GetParameters()
                .FirstOrDefault(p => p.GetCustomAttribute<FromRequestBodyAttribute>() != null);
        }

        private IOpenApiRequestBody CreateRequestBody(ParameterInfo requestBodyParameter, GeneratorContext generatorContext)
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>()
            };

            var schema = GetOrCreateSchema(requestBodyParameter, generatorContext);
            var examples = GenerateExamples(requestBodyParameter);

            var mediaType = new OpenApiMediaType
            {
                Schema = schema,
                Examples = examples,
            };

            // Add common content types for request bodies
            var contentTypes = new[] { "application/json", "application/xml", "text/plain" };
            foreach (var contentType in contentTypes)
            {
                // For now, we primarily support JSON
                if (contentType == "application/json")
                {
                    requestBody.Content.Add(contentType, mediaType);
                }
            }

            return requestBody;
        }

        private IOpenApiSchema GetOrCreateSchema(ParameterInfo requestBodyParameter, GeneratorContext generatorContext)
        {
            var parameterType = requestBodyParameter.ParameterType;
            var schemaKey = parameterType.FullName ?? parameterType.Name;

            if (generatorContext.Document.Components!.Schemas!.ContainsKey(schemaKey))
            {
                _logger.LogDebug("Using existing schema reference for type: {TypeName}", parameterType.FullName);
                return new OpenApiSchemaReference(schemaKey);
            }

            var schema = _schemaGenerator.GenerateSchema(parameterType, generatorContext);

            return schema ?? new OpenApiSchema { Type = JsonSchemaType.Object };
        }

        private IDictionary<string, IOpenApiExample>? GenerateExamples(ParameterInfo requestBodyParameter)
        {
            var exampleAttributes = requestBodyParameter.GetCustomAttributes<ExampleAttribute>();
            if (exampleAttributes == null || !exampleAttributes.Any())
            {
                _logger.LogDebug("No example attributes found for parameter: {ParameterName}", requestBodyParameter.Name);
                return null;
            }

            _logger.LogDebug("Generating examples for parameter: {ParameterName}", requestBodyParameter.Name);
            
            try
            {
                var examples = _examplesGenerator.GenerateExamples(exampleAttributes);
                return examples;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate examples for parameter: {ParameterName}", requestBodyParameter.Name);
                return null; // Don't fail the entire request body generation for example issues
            }
        }
    }
}