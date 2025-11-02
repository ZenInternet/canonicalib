using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultResponsesGenerator : IResponsesGenerator
    {
        private readonly IExamplesGenerator ExamplesGenerator;
        private readonly IHeadersGenerator HeadersGenerator;
        private readonly ISchemaGenerator SchemaGenerator;

        public DefaultResponsesGenerator(IExamplesGenerator examplesGenerator, IHeadersGenerator headersGenerator, ISchemaGenerator schemaGenerator)
        {
            ExamplesGenerator = examplesGenerator;
            HeadersGenerator = headersGenerator;
            SchemaGenerator = schemaGenerator;
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
                var responseType = attribute.Type;
                var exampleAttributes = endpointExamples.Where(x => x.StatusCode == attribute.StatusCode);
                var headerAttributes = endpointHeaders.Where(x => x.StatusCode == attribute.StatusCode);

                IDictionary<string, IOpenApiExample>? examples = exampleAttributes != null ? ExamplesGenerator.GenerateExamples(exampleAttributes) : null;
                
                IDictionary<string, IOpenApiHeader>? headers = headerAttributes != null ? HeadersGenerator.GenerateHeaders(headerAttributes, generatorContext) : null;
                
                var schema = SchemaGenerator.GenerateSchema(responseType, generatorContext);
                responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType()
                            {
                                Schema = schema,
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