using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultComponentsGenerator : IComponentsGenerator
    {

        private readonly ISchemasGenerator SchemasGenerator;

        public DefaultComponentsGenerator(ISchemasGenerator schemasGenerator)
        {
            SchemasGenerator = schemasGenerator;
        }

        public void GenerateComponents(GeneratorContext generatorContext)
        {
            SchemasGenerator.GenerateSchemas(generatorContext);
            generatorContext.Document.Components = new OpenApiComponents()
            {
                Schemas = generatorContext.Schemas
            };
        }
    }
}
