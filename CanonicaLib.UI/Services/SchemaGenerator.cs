using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Zen.CanonicaLib.UI.Services
{
    public class SchemaGenerator
    {

        public IOpenApiSchema GenerateSchema(Type schemaDefinition, Type contextType, Assembly contextAssembly)
        {
            var mappings = new Dictionary<Type, OpenApiSchema>
            {
                { typeof(string), new OpenApiSchema { Type = JsonSchemaType.String } },
                { typeof(int), new OpenApiSchema { Type = JsonSchemaType.Integer } },
                { typeof(bool), new OpenApiSchema { Type = JsonSchemaType.Boolean } },
                { typeof(double), new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" } },
                { typeof(float), new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" } },
                { typeof(DateTime), new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" } },
                { typeof(Guid), new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" } }
            };

            if (schemaDefinition == null)
                throw new ArgumentNullException(nameof(schemaDefinition));

            if (mappings.ContainsKey(schemaDefinition))
                return mappings[schemaDefinition];

            var listTypes = new List<Type> { typeof(List<>), typeof(IEnumerable<>), typeof(IList<>) };
            var genericTypeDefinition = schemaDefinition.IsGenericType ? schemaDefinition.GetGenericTypeDefinition() : null;

            if (schemaDefinition.IsArray || (schemaDefinition.IsGenericType && genericTypeDefinition != null && listTypes.Contains(genericTypeDefinition)))
            {
                var itemType = schemaDefinition.GetElementType() ?? schemaDefinition.GenericTypeArguments?[0];
                return new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = GenerateSchema(itemType, contextType, contextAssembly)
                };
            }
            
            if (schemaDefinition.Assembly == contextAssembly && schemaDefinition != contextType)
            {
                return new OpenApiSchemaReference(schemaDefinition.ToString().Replace(".", "_"));
            }

            var schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>(),
            };

            foreach (var property in schemaDefinition.GetProperties())
            {
                var jsonPropertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();

                schema.Properties.Add(
                    jsonPropertyNameAttribute != null ? jsonPropertyNameAttribute.Name : property.Name,
                    GenerateSchema(property.PropertyType, schemaDefinition, contextAssembly)
                );
            }

            return schema;
        }
    }
}