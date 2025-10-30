using Microsoft.OpenApi;
using System.Text.Json;
using System.Text.Json.Nodes;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultExamplesGenerator : IExamplesGenerator
    {
        public void GenerateExample(ExampleAttribute exampleAttribute, out IOpenApiExample example)
        {
            var valueString = JsonSerializer.Serialize(exampleAttribute.GetExampleContent());
            example = new OpenApiExample()
            {
                Description = exampleAttribute.GetDescription(),
                Value = JsonNode.Parse(valueString),
            };
        }

        public void GenerateExamples(IEnumerable<ExampleAttribute> exampleAttributes, out IDictionary<string, IOpenApiExample>? examples)
        {
            if (!exampleAttributes.Any())
            {
                examples = null;
                return;
            }
            examples = new Dictionary<string, IOpenApiExample>();
            foreach (var exampleAttr in exampleAttributes)
            {
                IOpenApiExample example;
                GenerateExample(exampleAttr, out example);
                examples.Add(exampleAttr.GetName(), example);
            }
        }
    }
}
