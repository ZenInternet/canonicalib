using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IParametersGenerator"/> that generates 
    /// OpenAPI parameters from method parameters.
    /// </summary>
    public sealed class DefaultParametersGenerator : IParametersGenerator
    {
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly CanonicaLibOptions _options;
        private readonly ILogger<DefaultParametersGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParametersGenerator"/> class.
        /// </summary>
        /// <param name="schemaGenerator">Generator for parameter schemas.</param>
        /// <param name="options">Configuration options for CanonicaLib.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultParametersGenerator(
            ISchemaGenerator schemaGenerator, 
            CanonicaLibOptions options,
            ILogger<DefaultParametersGenerator> logger)
        {
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates OpenAPI parameters from the method's parameter definitions.
        /// </summary>
        /// <param name="endpointDefinition">The method definition to analyze for parameters.</param>
        /// <param name="generatorContext">The context containing schemas and assembly information.</param>
        /// <returns>A list of OpenAPI parameters if any are found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public IList<IOpenApiParameter>? GenerateParameters(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            if (endpointDefinition == null)
                throw new ArgumentNullException(nameof(endpointDefinition));
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            _logger.LogDebug("Generating parameters for endpoint: {EndpointName}", endpointDefinition.Name);

            try
            {
                var endpointParameters = FindParameterCandidates(endpointDefinition);
                
                if (endpointParameters.Count == 0)
                {
                    _logger.LogDebug("No parameter candidates found for endpoint: {EndpointName}", endpointDefinition.Name);
                    return null;
                }

                var parameters = GenerateParameterList(endpointParameters, generatorContext);
                var processedParameters = ApplyPostProcessing(parameters);

                if (processedParameters.Count == 0)
                {
                    _logger.LogDebug("No parameters generated after processing for endpoint: {EndpointName}", endpointDefinition.Name);
                    return null;
                }

                _logger.LogDebug("Successfully generated {Count} parameters for endpoint: {EndpointName}", 
                    processedParameters.Count, endpointDefinition.Name);
                return processedParameters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate parameters for endpoint: {EndpointName}", endpointDefinition.Name);
                throw new InvalidOperationException($"Failed to generate parameters for endpoint '{endpointDefinition.Name}'", ex);
            }
        }

        private static List<ParameterInfo> FindParameterCandidates(MethodInfo endpointDefinition)
        {
            return endpointDefinition.GetParameters()
                .Where(p => p.GetCustomAttribute<OpenApiParameterAttribute>() != null)
                .ToList();
        }

        private List<IOpenApiParameter> GenerateParameterList(List<ParameterInfo> endpointParameters, GeneratorContext generatorContext)
        {
            var parameters = new List<IOpenApiParameter>();

            foreach (var endpointParameter in endpointParameters)
            {
                try
                {
                    var parameter = GenerateParameter(endpointParameter, generatorContext);
                    parameters.Add(parameter);
                    _logger.LogTrace("Generated parameter: {ParameterName}", endpointParameter.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate parameter: {ParameterName}", endpointParameter.Name);
                    // Continue with other parameters instead of failing completely
                }
            }

            return parameters;
        }

        private IList<IOpenApiParameter> ApplyPostProcessing(List<IOpenApiParameter> parameters)
        {
            if (_options.PostProcessors?.ParametersProcessor == null)
                return parameters;

            try
            {
                _logger.LogDebug("Applying parameters post-processor");
                var processedParameters = _options.PostProcessors.ParametersProcessor(parameters);
                return processedParameters?.ToList() ?? new List<IOpenApiParameter>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Parameters post-processor failed, returning original parameters");
                return parameters;
            }
        }

        private IOpenApiParameter GenerateParameter(ParameterInfo endpointParameter, GeneratorContext generatorContext)
        {
            var parameterAttribute = endpointParameter.GetCustomAttribute<OpenApiParameterAttribute>();
            if (parameterAttribute == null)
                throw new InvalidOperationException($"Parameter '{endpointParameter.Name}' missing OpenApiParameterAttribute");

            var parameterLocation = ParseParameterLocation(parameterAttribute.In);
            var schema = GenerateParameterSchema(endpointParameter, generatorContext);

            var parameter = new OpenApiParameter
            {
                Name = parameterAttribute.Name ?? endpointParameter.Name ?? "unknown",
                Description = parameterAttribute.Description,
                In = parameterLocation,
                Required = DetermineIfRequired(parameterAttribute, endpointParameter),
                Schema = schema
            };

            return parameter;
        }

        private static ParameterLocation ParseParameterLocation(string? locationString)
        {
            return locationString?.ToLowerInvariant() switch
            {
                "path" => ParameterLocation.Path,
                "query" => ParameterLocation.Query,
                "header" => ParameterLocation.Header,
                "cookie" => ParameterLocation.Cookie,
                _ => ParameterLocation.Query // Default fallback
            };
        }

        private IOpenApiSchema GenerateParameterSchema(ParameterInfo endpointParameter, GeneratorContext generatorContext)
        {
            try
            {
                var schema = _schemaGenerator.GenerateSchema(endpointParameter.ParameterType, generatorContext);
                return schema ?? CreateFallbackSchema(endpointParameter.ParameterType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate schema for parameter: {ParameterName}, using fallback", endpointParameter.Name);
                return CreateFallbackSchema(endpointParameter.ParameterType);
            }
        }

        private static IOpenApiSchema CreateFallbackSchema(Type parameterType)
        {
            return new OpenApiSchema
            {
                Type = parameterType == typeof(string) ? JsonSchemaType.String : JsonSchemaType.Object,
                Description = $"Schema for {parameterType.Name}"
            };
        }

        private static bool DetermineIfRequired(OpenApiParameterAttribute parameterAttribute, ParameterInfo endpointParameter)
        {
            // Use explicit Required value if set, otherwise check if parameter has default value
            return parameterAttribute?.Required ?? !endpointParameter.IsOptional;
        }
    }
}