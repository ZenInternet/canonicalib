using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IOperationGenerator
    {
        OpenApiOperation GenerateOperation(MethodInfo endpointDefinition, GeneratorContext generatorContext);
    }
}