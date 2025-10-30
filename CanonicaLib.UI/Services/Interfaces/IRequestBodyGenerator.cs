using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IRequestBodyGenerator
    {
        IOpenApiRequestBody? GenerateRequestBody(MethodInfo endpointDefinition, GeneratorContext generatorContext);
    }
}