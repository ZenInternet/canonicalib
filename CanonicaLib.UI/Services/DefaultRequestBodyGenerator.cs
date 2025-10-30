using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultRequestBodyGenerator : IRequestBodyGenerator
    {
        private readonly ISchemaGenerator SchemaGenerator;
        private readonly IExamplesGenerator ExamplesGenerator;

        public DefaultRequestBodyGenerator(ISchemaGenerator schemaGenerator, IExamplesGenerator examplesGenerator)
        {
            SchemaGenerator = schemaGenerator;
            ExamplesGenerator = examplesGenerator;
        }

        public IOpenApiRequestBody? GenerateRequestBody(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            // Find the parameter with the [FromRequestBody] attribute
            var requestBodyParameter = endpointDefinition.GetParameters()
                .FirstOrDefault(p => p.GetCustomAttribute<FromRequestBodyAttribute>() != null);

            var exampleAttributes = requestBodyParameter?.GetCustomAttributes<ExampleAttribute>();

            if (requestBodyParameter == null)
            {
                return null;
            }

            var requestBody = new OpenApiRequestBody()
            {
                Content = new Dictionary<string, OpenApiMediaType>()
            };

            IOpenApiSchema? schema = null;
            if (generatorContext.Schemas.ContainsKey(requestBodyParameter.ParameterType.FullName))
            {
                schema = new OpenApiSchemaReference(requestBodyParameter.ParameterType.FullName);
            }
            else
            {
                SchemaGenerator.GenerateSchema(requestBodyParameter.ParameterType, generatorContext, out schema);
            }

            IDictionary<string, IOpenApiExample>? examples = null;
            if (exampleAttributes != null)
            {
                ExamplesGenerator.GenerateExamples(exampleAttributes, out examples);
            }

            var mediaType = new OpenApiMediaType()
            {
                Schema = schema,
                Examples = examples,
            };

            requestBody.Content.Add("application/json", mediaType);

            return requestBody;
        }
    }
}