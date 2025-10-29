using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;

namespace Zen.CanonicaLib.UI.Services
{
    public class OperationGenerator
    {
        private readonly RequestBodyGenerator RequestBodyGenerator;
        private readonly ResponsesGenerator ResponsesGenerator;

        public OperationGenerator(
            RequestBodyGenerator requestBodyGenerator,
            ResponsesGenerator responsesGenerator)
        {
            RequestBodyGenerator = requestBodyGenerator;
            ResponsesGenerator = responsesGenerator;
        }

        public OpenApiOperation GenerateOperation(MethodInfo endpointDefinition, GeneratorContext generatorContext)
        {
            var tagAttribute = endpointDefinition.DeclaringType!.GetCustomAttribute<OpenApiTagAttribute>();

            var tags = tagAttribute != null ? new HashSet<OpenApiTagReference>()
            {
                new OpenApiTagReference(tagAttribute.Tag)
            } : null;

            var operation = new OpenApiOperation()
            {
                OperationId = $"{endpointDefinition.DeclaringType!.Name.Replace("I", "").Replace("Controller", "")}_{endpointDefinition.Name}",
                Tags = tags,
                Summary = endpointDefinition.GetXmlDocsSummary().IfEmpty(endpointDefinition.Name),
                Description = endpointDefinition.GetXmlDocsRemarks().IfEmpty(null),
                RequestBody = RequestBodyGenerator.GenerateRequestBody(endpointDefinition, generatorContext),
                Responses = ResponsesGenerator.GenerateResponses(endpointDefinition)
            };

            return operation;
        }
    }
}