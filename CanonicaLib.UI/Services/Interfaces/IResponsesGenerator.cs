using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IResponsesGenerator
    {
        OpenApiResponses GenerateResponses(MethodInfo endpointDefinition, GeneratorContext generatorContext);
    }
}