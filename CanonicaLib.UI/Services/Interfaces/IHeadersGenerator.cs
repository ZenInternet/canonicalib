using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IHeadersGenerator
    {
        IDictionary<string, IOpenApiHeader>? GenerateHeaders(IEnumerable<ResponseHeaderAttribute> headerAttributes, GeneratorContext generatorContext);
    }
}