using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Zen.CanonicaLib.UI.Services
{
    public class ExamplesGenerator
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

        internal void GenerateExamples(IEnumerable<ExampleAttribute> exampleAttributes, out IDictionary<string, IOpenApiExample> examples)
        {
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
