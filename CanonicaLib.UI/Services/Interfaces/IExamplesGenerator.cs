using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IExamplesGenerator
    {
        void GenerateExample(ExampleAttribute exampleAttribute, out IOpenApiExample example);
        void GenerateExamples(IEnumerable<ExampleAttribute> exampleAttributes, out IDictionary<string, IOpenApiExample>? examples);
    }
}