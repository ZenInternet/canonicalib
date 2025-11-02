using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IServersGenerator
    {
        IList<OpenApiServer>? GenerateServers(GeneratorContext generatorContext);
    }
}
