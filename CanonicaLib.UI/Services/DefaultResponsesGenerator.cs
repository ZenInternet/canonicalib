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

                if (responseType.FullName == "Systme.Void")
                    continue;

                var schema = SchemaGenerator.GenerateSchema(responseType, generatorContext);

                if (!responses.ContainsKey(statusCode))
                {
                    responses[statusCode] = new OpenApiResponse
                    {
                        Description = description,
                        Content = new Dictionary<string, OpenApiMediaType>(),
                        Headers = new Dictionary<string, IOpenApiHeader>(),
                    };
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (!responses[statusCode]!.Headers!.ContainsKey(header.Key))
                        {
                            responses[statusCode]!.Headers!.Add(header.Key, header.Value);
                        }
                    }
                }

                if (!responses[statusCode]!.Content!.ContainsKey("application/json"))
                {
                    responses[statusCode]!.Content!["application/json"] = new OpenApiMediaType
                    {
                        Schema = schema,
                        Examples = examples,
                    };
                    continue;
                }

                if (responses[statusCode]!.Content!["application/json"].Schema!.OneOf == null)
                {
                    var existingSchema = responses[statusCode]!.Content!["application/json"].Schema;
                    responses[statusCode]!.Content!["application/json"].Schema = new OpenApiSchema
                    {
                        OneOf = new List<IOpenApiSchema> { existingSchema!, schema! }
                    };
                }
                else
                {
                    responses[statusCode]!.Content!["application/json"].Schema!.OneOf!.Add(schema!);
                }

                if (examples != null)
                {
                    foreach (var example in examples)
                    {
                        if (!responses[statusCode]!.Content!["application/json"].Examples!.ContainsKey(example.Key))
                        {
                            responses[statusCode]!.Content!["application/json"].Examples!.Add(example.Key, example.Value);
                        }
                    }
                }
            }
            return responses;
        }
    }
}