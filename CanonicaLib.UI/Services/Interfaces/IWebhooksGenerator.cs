using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IWebhooksGenerator
    {
        IDictionary<string, IOpenApiPathItem>? GenerateWebhooks(GeneratorContext generatorContext);
    }
}