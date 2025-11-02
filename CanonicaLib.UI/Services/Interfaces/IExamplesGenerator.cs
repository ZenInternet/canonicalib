using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IExamplesGenerator
    {
        IOpenApiExample GenerateExample(ExampleAttribute exampleAttribute);
        IDictionary<string, IOpenApiExample>? GenerateExamples(IEnumerable<ExampleAttribute> exampleAttributes);
    }
}