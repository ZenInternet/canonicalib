using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.OpenApiExtensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultOperationGenerator : IOperationGenerator
    {
        private readonly IRequestBodyGenerator RequestBodyGenerator;
        private readonly IParametersGenerator ParametersGenerator;
        private readonly IResponsesGenerator ResponsesGenerator;
        private readonly ISecurityGenerator SecurityGenerator;

        public DefaultOperationGenerator(
            IRequestBodyGenerator requestBodyGenerator,
            IParametersGenerator parametersGenerator,
            IResponsesGenerator responsesGenerator,
            ISecurityGenerator securityGenerator)
        {
            RequestBodyGenerator = requestBodyGenerator;
            ParametersGenerator = parametersGenerator;
            ResponsesGenerator = responsesGenerator;
            SecurityGenerator = securityGenerator;
        }

        public OpenApiOperation GenerateOperation(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            var tagAttribute = endpointDefinition.DeclaringType!.GetCustomAttribute<OpenApiTagAttribute>()
                ?? endpointDefinition.DeclaringType!.GetInterfaces()
                    .Select(i => i.GetCustomAttribute<OpenApiTagAttribute>())
                    .FirstOrDefault(a => a != null);

            var tags = tagAttribute != null ? new HashSet<OpenApiTagReference>()
            {
                new OpenApiTagReference(tagAttribute.Tag)
            } : null;

            var operation = new OpenApiOperation()
            {
                OperationId = $"{endpointDefinition.DeclaringType!.Name.Replace("I", "").Replace("Controller", "")}_{endpointDefinition.Name}",
                Tags = tags,
                Summary = endpointDefinition.GetXmlDocsSummary().IfEmpty(endpointDefinition.Name),
                Description = (endpointDefinition.GetXmlDocsRemarksPreservingLineBreaks() ?? string.Empty).IfEmpty(null),
                RequestBody = RequestBodyGenerator.GenerateRequestBody(endpointDefinition, generatorContext),
                Parameters = ParametersGenerator.GenerateParameters(endpointDefinition, generatorContext),
                Responses = ResponsesGenerator.GenerateResponses(endpointDefinition, generatorContext),
                Security = SecurityGenerator.GenerateOperationSecurityRequirements(endpointDefinition)
            };

            var mcpAttribute = endpointDefinition.GetCustomAttribute<OpenApiMcpAttribute>();
            if (mcpAttribute != null)
            {
                operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                operation.Extensions.Add("x-mcp", new McpExtension(mcpAttribute));
            }

            return operation;
        }
    }
}