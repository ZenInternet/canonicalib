using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IInfoGenerator
    {
        OpenApiInfo GenerateInfo(GeneratorContext generatorContext);
    }
}