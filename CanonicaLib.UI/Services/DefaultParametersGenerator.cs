using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultParametersGenerator : IParametersGenerator
    {
        private readonly ISchemaGenerator SchemaGenerator;

        private readonly CanonicaLibOptions Options;

        public DefaultParametersGenerator(ISchemaGenerator schemaGenerator, CanonicaLibOptions options)
        {
            SchemaGenerator = schemaGenerator;
            Options = options;
        }

        public IList<IOpenApiParameter>? GenerateParameters(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            // Find the parameter without the [FromRequestBody] attribute
            var endpointParameters = endpointDefinition.GetParameters()
                .Where(p => p.GetCustomAttribute<OpenApiParameterAttribute>() != null)
                .ToList();

            var parameters = new List<IOpenApiParameter>();

            foreach (var endpointParameter in endpointParameters)
            {
                parameters.Add(GenerateParameter(endpointParameter, generatorContext));
            }

            if (Options.PostProcessors.ParametersProcessor != null)
            {
                parameters = Options.PostProcessors.ParametersProcessor(parameters)?.ToList() ?? new List<IOpenApiParameter>();
            }

            if (parameters.Count == 0)
                return null;

            return parameters;
        }

        private IOpenApiParameter GenerateParameter(ParameterInfo endpointParameter, GeneratorContext generatorContext)
        {
            var parameterAttribute = endpointParameter.GetCustomAttribute<OpenApiParameterAttribute>();

            var parameterLocation = ParameterLocation.Path;
            switch (parameterAttribute?.In)
            {
                case "path":
                    parameterLocation = ParameterLocation.Path;
                    break;
                case "query":
                    parameterLocation = ParameterLocation.Query;
                    break;
                case "header":
                    parameterLocation = ParameterLocation.Header;
                    break;
            }

            IOpenApiSchema? schema;
            SchemaGenerator.GenerateSchema(endpointParameter.ParameterType, generatorContext, out schema);

            var parameter = new OpenApiParameter()
            {
                Name = parameterAttribute?.Name ?? endpointParameter.Name,
                Description = parameterAttribute?.Description,
                In = parameterLocation,
                Required = parameterAttribute?.Required ?? !endpointParameter.IsOptional,
                Schema = schema
            };

            return parameter;
        }

    }
}