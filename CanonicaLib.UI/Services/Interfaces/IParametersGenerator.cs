using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IParametersGenerator
    {
        IList<IOpenApiParameter>? GenerateParameters(MethodInfo endpointDefinition, GeneratorContext generatorContext);
    }
}