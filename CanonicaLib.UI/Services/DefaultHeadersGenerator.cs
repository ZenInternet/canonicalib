using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IHeadersGenerator"/> that generates 
    /// OpenAPI headers from response header attributes.
    /// </summary>
    public sealed class DefaultHeadersGenerator : IHeadersGenerator
    {
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly CanonicaLibOptions _options;
        private readonly ILogger<DefaultHeadersGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHeadersGenerator"/> class.
        /// </summary>
        /// <param name="schemaGenerator">Generator for header schemas.</param>
        /// <param name="options">Configuration options for CanonicaLib.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public DefaultHeadersGenerator(
            ISchemaGenerator schemaGenerator, 
            CanonicaLibOptions options,
            ILogger<DefaultHeadersGenerator> logger)
        {
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates OpenAPI headers from response header attributes.
        /// </summary>
        /// <param name="headerAttributes">The collection of response header attributes.</param>
        /// <param name="generatorContext">The context containing schemas and assembly information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public IDictionary<string, IOpenApiHeader>? GenerateHeaders(IEnumerable<ResponseHeaderAttribute>? headerAttributes, GeneratorContext generatorContext)
        {
            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            var attributesList = headerAttributes?.ToList();
            _logger.LogDebug("Generating headers from {Count} header attributes", attributesList?.Count);

            try
            {
                if (attributesList?.Count == 0)
                {
                    _logger.LogDebug("No header attributes provided, returning null headers");
                    return null;
                }

                var generatedHeaders = GenerateHeaderDictionary(attributesList, generatorContext);
                var processedHeaders = ApplyPostProcessing(generatedHeaders);

                return processedHeaders?.Count > 0 ? processedHeaders : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate headers");
                throw new InvalidOperationException("Failed to generate headers from attributes", ex);
            }
        }

        private IDictionary<string, IOpenApiHeader> GenerateHeaderDictionary(
            List<ResponseHeaderAttribute>? headerAttributes, 
            GeneratorContext generatorContext)
        {
            var headers = new Dictionary<string, IOpenApiHeader>();
            if (headerAttributes == null || headerAttributes.Count == 0)
                return headers;

            var processedNames = new HashSet<string>();

            foreach (var headerAttribute in headerAttributes)
            {
                try
                {
                    var headerName = EnsureUniqueHeaderName(headerAttribute.Name, processedNames);
                    var header = GenerateHeader(headerAttribute, generatorContext);
                    
                    if (header != null)
                    {
                        headers.Add(headerName, header);
                        processedNames.Add(headerName);
                        _logger.LogTrace("Generated header: {HeaderName}", headerName);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to generate header for: {HeaderName}", headerAttribute.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate header: {HeaderName}", headerAttribute.Name);
                    // Continue with other headers instead of failing completely
                }
            }

            return headers;
        }

        private IDictionary<string, IOpenApiHeader>? ApplyPostProcessing(IDictionary<string, IOpenApiHeader> headers)
        {
            if (_options.PostProcessors?.HeadersProcessor == null)
                return headers;

            try
            {
                _logger.LogDebug("Applying headers post-processor");
                var processedHeaders = _options.PostProcessors.HeadersProcessor(headers);
                return processedHeaders?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Headers post-processor failed, returning original headers");
                return headers;
            }
        }

        private IOpenApiHeader? GenerateHeader(ResponseHeaderAttribute headerAttribute, GeneratorContext generatorContext)
        {
            if (headerAttribute.Type == null)
            {
                _logger.LogWarning("Header attribute has null Type for header: {HeaderName}", headerAttribute.Name);
                return null;
            }

            try
            {
                var schema = _schemaGenerator.GenerateSchema(headerAttribute.Type, generatorContext);
                
                if (schema == null)
                {
                    _logger.LogWarning("Failed to generate schema for header type: {TypeName}", headerAttribute.Type.FullName);
                    return null;
                }

                return new OpenApiHeader
                {
                    Description = headerAttribute.Description,
                    Schema = schema,
                    Required = DetermineIfRequired(headerAttribute),
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate header schema for type: {TypeName}", headerAttribute.Type.FullName);
                return null;
            }
        }

        private static string EnsureUniqueHeaderName(string baseName, ISet<string> existingNames)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "X-Custom-Header";

            var uniqueName = baseName;
            var counter = 1;

            while (existingNames.Contains(uniqueName))
            {
                uniqueName = $"{baseName}-{counter}";
                counter++;
            }

            return uniqueName;
        }

        private static bool DetermineIfRequired(ResponseHeaderAttribute headerAttribute)
        {
            // Headers are typically optional unless explicitly marked as required
            // This could be enhanced with additional attributes or conventions
            return false;
        }
    }
}
