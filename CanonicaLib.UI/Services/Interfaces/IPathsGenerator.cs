using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IPathsGenerator
    {
        OpenApiPaths GeneratePaths(GeneratorContext generatorContext);
    }
}