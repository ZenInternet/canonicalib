using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.OpenApiExtensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Default implementation of <see cref="IExamplesGenerator"/> that generates 
    /// OpenAPI examples from <see cref="ExampleAttribute"/> instances.
    /// </summary>
    public sealed class DefaultExamplesGenerator : IExamplesGenerator
    {
        private readonly ILogger<DefaultExamplesGenerator> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExamplesGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="serviceProvider">Service provider for validation context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> or <paramref name="serviceProvider"/> is null.</exception>
        public DefaultExamplesGenerator(ILogger<DefaultExamplesGenerator> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Generates a single OpenAPI example from an example attribute.
        /// </summary>
        /// <param name="exampleAttribute">The example attribute containing example data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exampleAttribute"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when example generation fails.</exception>
        public IOpenApiExample GenerateExample(ExampleAttribute exampleAttribute)
        {
            if (exampleAttribute == null)
                throw new ArgumentNullException(nameof(exampleAttribute));

            _logger.LogDebug("Generating example from type: {ExampleType}", exampleAttribute.ExampleType.FullName);

            try
            {
                // Use the enhanced method with service provider support
                IList<ValidationResult> validationResults;
                var exampleContent = exampleAttribute.GetExampleContent(out validationResults, _serviceProvider);
                var valueString = JsonSerializer.Serialize(exampleContent, _jsonOptions);

                var example = new OpenApiExample
                {
                    Description = exampleAttribute.GetDescription(),
                    Value = JsonNode.Parse(valueString),
                };

                if (validationResults.Any())
                {
                    example.AddExtension("x-validation-results", new ValidationResultsExtension(validationResults));
                }

                return example;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate example for type: {ExampleType}", exampleAttribute.ExampleType.FullName);
                throw new InvalidOperationException($"Failed to generate example for type '{exampleAttribute.ExampleType.FullName}'", ex);
            }
        }

        /// <summary>
        /// Generates a collection of OpenAPI examples from multiple example attributes.
        /// </summary>
        /// <param name="exampleAttributes">The collection of example attributes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exampleAttributes"/> is null.</exception>
        public IDictionary<string, IOpenApiExample>? GenerateExamples(IEnumerable<ExampleAttribute> exampleAttributes)
        {
            if (exampleAttributes == null)
                throw new ArgumentNullException(nameof(exampleAttributes));

            var attributesList = exampleAttributes.ToList();
            
            if (attributesList.Count == 0)
            {
                _logger.LogDebug("No example attributes provided, returning null examples");
                return null;
            }

            _logger.LogDebug("Generating {Count} examples", attributesList.Count);

            try
            {
                var examples = new Dictionary<string, IOpenApiExample>();
                var processedNames = new HashSet<string>();

                foreach (var exampleAttr in attributesList)
                {
                    var exampleName = exampleAttr.GetName();
                    
                    // Handle duplicate names by appending a counter
                    var uniqueName = EnsureUniqueName(exampleName, processedNames);
                    processedNames.Add(uniqueName);

                    var example = GenerateExample(exampleAttr);
                    examples.Add(uniqueName, example);
                }

                if (examples.Count == 0)
                {
                    _logger.LogDebug("No examples were generated after processing attributes, returning null examples");
                    return null;
                }

                return examples;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate examples collection");
                throw new InvalidOperationException("Failed to generate examples collection", ex);
            }
        }

        private static string EnsureUniqueName(string baseName, ISet<string> existingNames)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "example";

            var uniqueName = baseName;
            var counter = 1;

            while (existingNames.Contains(uniqueName))
            {
                uniqueName = $"{baseName}_{counter}";
                counter++;
            }

            return uniqueName;
        }
    }
}
