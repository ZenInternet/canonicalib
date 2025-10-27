using Microsoft.OpenApi;
using System.Reflection;

namespace CanonicaLib.UI.Services
{
    public class ComponentsGenerator
    {

        private readonly SchemasGenerator SchemasGenerator;

        public ComponentsGenerator(SchemasGenerator schemasGenerator)
        {
            SchemasGenerator = schemasGenerator;
        }

        public OpenApiComponents GenerateComponents(Assembly assembly) => new OpenApiComponents()
        {
            Schemas = SchemasGenerator.GenerateSchemas(assembly)
        };
    }
}
