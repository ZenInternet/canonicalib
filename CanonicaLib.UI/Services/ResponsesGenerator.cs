using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services
{
    public class ResponsesGenerator
    {
        private readonly ExamplesGenerator ExamplesGenerator;

        public ResponsesGenerator(ExamplesGenerator examplesGenerator)
        {
            ExamplesGenerator = examplesGenerator;
        }

        public OpenApiResponses GenerateResponses(MethodInfo endpointDefinition)
        {
            var responses = new OpenApiResponses();

            var attributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
            var endpointExamples = endpointDefinition.GetCustomAttributes<ResponseExampleAttribute>();
            foreach (var attribute in attributes)
            {
                var statusCode = attribute.StatusCode.ToString();
                var description = attribute.Description ?? string.Empty;
                var responseType = attribute.Type?.ToString();
                var exampleAttributes = endpointExamples.Where(x => x.StatusCode == attribute.StatusCode);

                IDictionary<string, IOpenApiExample>? examples = null;
                if (exampleAttributes != null)
                {
                    ExamplesGenerator.GenerateExamples(exampleAttributes, out examples);
                }

                responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType()
                            {
                                Schema = responseType != null ? new OpenApiSchemaReference(responseType) : null,
                                Examples = examples
                            }
                        }
                    }
                };
            }

            return responses;
        }
    }
}