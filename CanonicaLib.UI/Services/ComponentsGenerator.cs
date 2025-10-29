using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class ComponentsGenerator
    {

        private readonly SchemasGenerator SchemasGenerator;

        public ComponentsGenerator(SchemasGenerator schemasGenerator)
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
