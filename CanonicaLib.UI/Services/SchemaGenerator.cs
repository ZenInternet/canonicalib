using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.OpenApiExtensions;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Generates OpenAPI schemas for types using reflection
    /// </summary>
    public class SchemaGenerator
    {
        public void GenerateSchema(Type schemaDefinition, GeneratorContext generatorContext, out IOpenApiSchema? openApiSchema)
        {
            if (schemaDefinition == null)
                throw new ArgumentNullException(nameof(schemaDefinition));

            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            var schemaKey = GetSchemaKey(schemaDefinition);

            try
            {
                // Generate the OpenAPI schema using reflection
                openApiSchema = CreateSchemaFromType(schemaDefinition, generatorContext.Schemas, generatorContext.Assembly);

                if (generatorContext.Schemas.ContainsKey(schemaKey))
                {
                    // Update the placeholder in the schemas dictionary
                    generatorContext.Schemas[schemaKey] = openApiSchema;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate schema for type '{schemaDefinition.FullName}'", ex);
            }
        }

        private static string GetSchemaKey(Type type)
        {
            return type.FullName ?? type.Name;
        }

        private OpenApiSchema CreateSchemaFromType(Type type, Dictionary<string, IOpenApiSchema> existingSchemas, Assembly targetAssembly)
        {
            var schema = new OpenApiSchema();

            var tagAttribute = type.GetCustomAttribute<OpenApiTagAttribute>();
            if (tagAttribute != null)
            {
                schema.Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    { "x-tags", new TagsExtension(tagAttribute.Tag) }
                };
            }

            // Handle nullable types
            if (IsNullableType(type))
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
            }

            // Handle primitive types
            if (IsPrimitiveType(type))
            {
                SetPrimitiveTypeProperties(schema, type);
                return schema;
            }

            // Handle arrays and collections
            if (IsArrayOrCollection(type))
            {
                schema.Type = JsonSchemaType.Array;
                var elementType = GetElementType(type);
                if (elementType != null)
                {
                    schema.Items = CreateSchemaOrReference(elementType, existingSchemas, targetAssembly);
                }
                return schema;
            }

            // Handle enum types
            if (type.IsEnum)
            {
                // Create inline enum schema if not in existing schemas
                schema.Type = JsonSchemaType.String;
                schema.Title = type.Name;
                schema.Description = type.GetXmlDocsSummary() ?? null;
                schema.Comment = type.GetXmlDocsRemarks() ?? null;
                schema.Enum = new List<System.Text.Json.Nodes.JsonNode>();
                foreach (var enumValue in Enum.GetValues(type))
                {
                    var enumString = enumValue.ToString();
                    if (enumString != null)
                    {
                        schema.Enum.Add(System.Text.Json.Nodes.JsonValue.Create(enumString));
                    }
                }
                return schema;
            }

            // Handle object types
            schema.Type = JsonSchemaType.Object;
            schema.Title = type.Name;
            schema.Description = type.GetXmlDocsSummary() ?? null;
            schema.Comment = type.GetXmlDocsRemarks() ?? null;
            schema.Properties = new Dictionary<string, IOpenApiSchema>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var requiredProperties = new HashSet<string>();

            foreach (var property in properties)
            {
                if (property.CanRead && property.GetGetMethod()?.IsPublic == true)
                {
                    var propertyName = GetPropertyName(property);
                    schema.Properties[propertyName] = CreateSchemaOrReference(property.PropertyType, existingSchemas, targetAssembly);

                    // Check if property is required (not nullable and no default value)
                    if (IsRequiredProperty(property))
                    {
                        requiredProperties.Add(propertyName);
                    }
                }
            }

            if (requiredProperties.Count > 0)
            {
                schema.Required = requiredProperties;
            }

            return schema;
        }

        private IOpenApiSchema CreateSchemaOrReference(Type type, Dictionary<string, IOpenApiSchema> existingSchemas, Assembly targetAssembly)
        {
            // Handle nullable types
            if (IsNullableType(type))
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
            }

            // For primitive types, create schema directly
            if (IsPrimitiveType(type))
            {
                return CreateSchemaFromType(type, existingSchemas, targetAssembly);
            }


            // For complex types, check if the type belongs to the target assembly
            if (type.Assembly != targetAssembly)
            {
                // For types not in the target assembly, create a simple object schema
                return new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    AdditionalPropertiesAllowed = true
                };
            }

            // For complex types in the target assembly, check if we should create a reference
            var typeKey = GetSchemaKey(type);
            if (existingSchemas.ContainsKey(typeKey))
            {
                return new OpenApiSchemaReference(typeKey);
            }

            // If it's not in our existing schemas but is in the target assembly, create schema directly
            return CreateSchemaFromType(type, existingSchemas, targetAssembly);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        private static void SetPrimitiveTypeProperties(OpenApiSchema schema, Type type)
        {
            if (type == typeof(string))
            {
                schema.Type = JsonSchemaType.String;
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                schema.Type = JsonSchemaType.Integer;
            }
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                schema.Type = JsonSchemaType.Number;
            }
            else if (type == typeof(bool))
            {
                schema.Type = JsonSchemaType.Boolean;
            }
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                schema.Type = JsonSchemaType.String;
                schema.Format = "date-time";
            }
            else if (type == typeof(Guid))
            {
                schema.Type = JsonSchemaType.String;
                schema.Format = "uuid";
            }
            else
            {
                schema.Type = JsonSchemaType.String;
            }
        }

        private static bool IsArrayOrCollection(Type type)
        {
            return type.IsArray ||
                   (type.IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(List<>) ||
                     type.GetGenericTypeDefinition() == typeof(IList<>) ||
                     type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                     type.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        private static Type? GetElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType)
            {
                return type.GetGenericArguments().FirstOrDefault();
            }

            return null;
        }

        private static string GetPropertyName(PropertyInfo property)
        {
            // Could be extended to check for JsonPropertyName attributes or similar
            return char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
        }

        private static bool IsRequiredProperty(PropertyInfo property)
        {
            // Simple logic - could be enhanced to check for Required attributes or nullable reference types
            var propertyType = property.PropertyType;

            // Value types (except nullable) are typically required
            if (propertyType.IsValueType && !IsNullableType(propertyType))
            {
                return true;
            }

            // Could add more sophisticated logic here based on attributes
            return false;
        }
    }
}