using CanonicaLib.UI.Extensions;
using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Reflection;

namespace CanonicaLib.UI.Services
{
    public class OperationGenerator
    {
        public OpenApiOperation GenerateOpration(MethodInfo endpointDefinition)
        {
            var operation = new OpenApiOperation()
            {
                OperationId = $"{endpointDefinition.DeclaringType!.Name.Replace("I", "").Replace("Controller", "")}_{endpointDefinition.Name}",
                Summary = endpointDefinition.GetXmlDocsSummary().IfEmpty(endpointDefinition.Name),
                Description = endpointDefinition.GetXmlDocsRemarks().IfEmpty(null),
                Responses = ResponsesGenerator.GenerateResponses(endpointDefinition)
            };

            return operation;
        }
    }
}