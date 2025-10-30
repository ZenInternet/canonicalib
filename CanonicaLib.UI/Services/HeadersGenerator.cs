using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services
{
    public class HeadersGenerator
    {
        private readonly SchemaGenerator SchemaGenerator;

        private readonly CanonicaLibOptions Options;

        public HeadersGenerator(SchemaGenerator schemaGenerator, CanonicaLibOptions options)
        {
            SchemaGenerator = schemaGenerator;
            Options = options;
        }

        public void GenerateHeaders(IEnumerable<ResponseHeaderAttribute> headerAttributes, GeneratorContext generatorContext, out IDictionary<string, IOpenApiHeader>? headers)
        {
            headers = new Dictionary<string, IOpenApiHeader>();
            foreach (var headerAttribute in headerAttributes)
            {
                var name = headerAttribute.Name;
                var header = GenerateHeader(headerAttribute, generatorContext);
                if (header != null)
                    headers.Add(name, header);
            }

            if (Options.PostProcessors?.HeadersProcessor != null)
            {
                headers = Options.PostProcessors.HeadersProcessor(headers)?.ToDictionary();
            }
             
            if (headers?.Count == 0)
            {
                headers = null;
                return;
            }
        }
        private IOpenApiHeader? GenerateHeader(ResponseHeaderAttribute headerAttribute, GeneratorContext generatorContext)
        {
            IOpenApiSchema? schema;
            SchemaGenerator.GenerateSchema(headerAttribute.Type, generatorContext, out schema);
            if (schema == null)
            {
                return null;
            }

            var header = new OpenApiHeader()
            {
                Description = headerAttribute.Description,
                Schema = schema,
            };

            return header;
        }
    }
}
