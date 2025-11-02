using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IComponentsGenerator
    {
        OpenApiComponents? GenerateComponents(GeneratorContext generatorContext);
    }
}