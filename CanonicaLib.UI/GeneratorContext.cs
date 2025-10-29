using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI
{
    public class GeneratorContext
    {
        public Assembly Assembly { get; init; }

        public OpenApiDocument Document { get; init; }

        public Dictionary<string, IOpenApiSchema> Schemas { get; set; } = new Dictionary<string, IOpenApiSchema>();

        public GeneratorContext(Assembly assembly)
        {
            Assembly = assembly;
            var document = new OpenApiDocument();
            document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            Document = document;
        }
    }
}
