using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IHeadersGenerator
    {
        void GenerateHeaders(IEnumerable<ResponseHeaderAttribute> headerAttributes, GeneratorContext generatorContext, out IDictionary<string, IOpenApiHeader>? headers);
    }
}