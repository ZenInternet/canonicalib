using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services
{
    public class ResponsesGenerator
    {
        private readonly ExamplesGenerator ExamplesGenerator;

        private readonly HeadersGenerator HeadersGenerator;

        public ResponsesGenerator(ExamplesGenerator examplesGenerator, HeadersGenerator headersGenerator)
        {
            ExamplesGenerator = examplesGenerator;
            HeadersGenerator = headersGenerator;
        }

        public OpenApiResponses GenerateResponses(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            var responses = new OpenApiResponses();

            var attributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
            var endpointExamples = endpointDefinition.GetCustomAttributes<ResponseExampleAttribute>();
            var endpointHeaders = endpointDefinition.GetCustomAttributes<ResponseHeaderAttribute>();

            foreach (var attribute in attributes)
            {
                var statusCode = attribute.StatusCode.ToString();
                var description = attribute.Description ?? string.Empty;
                var responseType = attribute.Type?.ToString();
                var exampleAttributes = endpointExamples.Where(x => x.StatusCode == attribute.StatusCode);
                var headerAttributes = endpointHeaders.Where(x => x.StatusCode == attribute.StatusCode);

                IDictionary<string, IOpenApiExample>? examples = null;
                if (exampleAttributes != null)
                {
                    ExamplesGenerator.GenerateExamples(exampleAttributes, out examples);
                }

                IDictionary<string, IOpenApiHeader>? headers = null;
                if (headerAttributes != null)
                {
                    HeadersGenerator.GenerateHeaders(headerAttributes, generatorContext, out headers);
                }

                responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType()
                            {
                                Schema = responseType != null ? new OpenApiSchemaReference(responseType) : null,
                                Examples = examples,
                            }
                        }
                    },
                    Headers = headers,
                };
            }

            return responses;
        }
    }
}