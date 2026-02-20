using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Collections.ObjectModel;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.OpenApiExtensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    /// <summary>
    /// Generates OpenAPI schemas for types using reflection
    /// </summary>
    public class DefaultSchemaGenerator : ISchemaGenerator
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger<DefaultSchemaGenerator> _logger;

        public DefaultSchemaGenerator(IDiscoveryService discoveryService, ILogger<DefaultSchemaGenerator> logger)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IOpenApiSchema? GenerateSchema(Type schemaDefinition, GeneratorContext generatorContext)
        {

            if (schemaDefinition == null)
                throw new ArgumentNullException(nameof(schemaDefinition));

            if (schemaDefinition.GetInterfaces().Any(x => x.Name == "IExample`1" || x.Name == "ILibrary" || x.Name == "IService"))
                throw new ArgumentException("Parameter can not implement IExample, ILibrary or IService", nameof(schemaDefinition));

            if (generatorContext == null)
                throw new ArgumentNullException(nameof(generatorContext));

            try
            {
                // Generate the OpenAPI schema using reflection
                var referenceType = _discoveryService.GetAssemblyReferenceType(generatorContext.Assembly, schemaDefinition);
                var schema = CreateSchemaFromType(schemaDefinition, referenceType, generatorContext);
                return schema;
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

        private IOpenApiSchema CreateSchemaFromType(Type type, AssemblyReferenceType referenceType, GeneratorContext generatorContext)
        {
            _logger.LogInformation("Creating schema for type: {TypeName}", type.FullName);
            var existingRef = generatorContext.GetExistingSchema(type);
            if (existingRef != null)
            {
                _logger.LogInformation("Schema for type {TypeName} already exists. Using existing schema reference.", type.FullName);
                return existingRef;
            }

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
                _logger.LogInformation("Type {TypeName} is nullable. Processing underlying type.", type.FullName);
                type = Nullable.GetUnderlyingType(type) ?? type;
                referenceType = _discoveryService.GetAssemblyReferenceType(generatorContext.Assembly, type);
            }

            // Handle primitive types
            if (IsPrimitiveType(type))
            {
                _logger.LogInformation("Type {TypeName} is a primitive type. Creating primitive schema.", type.FullName);
                SetPrimitiveTypeProperties(schema, type);
                return schema;
            }

            // Handle Dictionary types
            if (IsDictionary(type))
            {
                _logger.LogInformation("Type {TypeName} is a dictionary. Creating object schema with additionalProperties.", type.FullName);
                schema.Type = JsonSchemaType.Object;

                // Find the IDictionary<,> interface to extract key and value types
                var dictionaryInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                Type? valueType = null;
                Type? keyType = null;

                if (dictionaryInterface != null)
                {
                    var genericArgs = dictionaryInterface.GetGenericArguments();
                    keyType = genericArgs[0];
                    valueType = genericArgs[1];
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var genericArgs = type.GetGenericArguments();
                    keyType = genericArgs[0];
                    valueType = genericArgs[1];
                }

                // Log warning if key type is not string
                if (keyType != null && keyType != typeof(string))
                {
                    _logger.LogWarning("Dictionary key type {KeyType} is not string. OpenAPI only supports string keys.", keyType.FullName);
                }

                // Set additionalProperties for the value type
                if (valueType != null)
                {
                    var valueReferenceType = _discoveryService.GetAssemblyReferenceType(generatorContext.Assembly, valueType);
                    schema.AdditionalProperties = CreateSchemaFromType(valueType, valueReferenceType, generatorContext);
                }

                // Try to add schema to context (for internal/external dictionaries)
                if (generatorContext.AddSchema(type, schema, referenceType))
                {
                    var schemaKey = GetSchemaKey(type);
                    return new OpenApiSchemaReference(schemaKey);
                }

                return schema;
            }

            // Handle arrays and collections
            // Array schemas are always inline, not registered as top-level components.
            // Recursion prevention is handled by the element type being registered early.
            if (IsArrayOrCollection(type))
            {
                schema.Type = JsonSchemaType.Array;
                var elementType = GetElementType(type);
                _logger.LogInformation("Type {TypeName} is an array or collection of {elementType}. Creating array schema.", type.FullName, elementType?.FullName);

                if (elementType != null)
                {
                    var elementReferenceType = _discoveryService.GetAssemblyReferenceType(generatorContext.Assembly, elementType);
                    schema.Items = CreateSchemaFromType(elementType, elementReferenceType, generatorContext);
                }

                return schema;
            }

            // Handle enum types
            if (type.IsEnum)
            {
                _logger.LogInformation("Type {TypeName} is an enum. Creating enum schema.", type.FullName);
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
                if (generatorContext.AddSchema(type, schema, referenceType))
                {
                    var schemaKey = GetSchemaKey(type);
                    return new OpenApiSchemaReference(schemaKey);
                }
                return schema;
            }

            // Handle object types
            schema.Type = JsonSchemaType.Object;
            schema.Title = type.Name;
            schema.Description = type.GetXmlDocsSummary() ?? null;
            schema.Comment = type.GetXmlDocsRemarks() ?? null;
            schema.Properties = new Dictionary<string, IOpenApiSchema>();

            // Add the schema to context BEFORE processing properties to prevent infinite recursion
            // when a type references itself (e.g., Entity with Children property of type Entity[])
            var schemaAddedEarly = generatorContext.AddSchema(type, schema, referenceType);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var requiredProperties = new HashSet<string>();

            foreach (var property in properties)
            {
                if (property.CanRead && property.GetGetMethod()?.IsPublic == true)
                {
                    var propertyName = GetPropertyName(property);

                    var propertyReferenceType = _discoveryService.GetAssemblyReferenceType(generatorContext.Assembly, property.PropertyType);
                    schema.Properties[propertyName] = CreateSchemaFromType(property.PropertyType, propertyReferenceType, generatorContext);

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

            // If the schema was added early to prevent recursion, return a reference
            // Otherwise, check if it should be added now
            if (schemaAddedEarly)
            {
                var schemaKey = GetSchemaKey(type);
                _logger.LogInformation("Type {TypeName} schema was added early to prevent recursion. Creating schema reference {schemaKey}.", type.FullName, schemaKey);
                return new OpenApiSchemaReference(schemaKey);
            }

            if (generatorContext.AddSchema(type, schema, referenceType))
            {
                var schemaKey = GetSchemaKey(type);
                _logger.LogInformation("Type {TypeName} schema was newly added to context. Creating schema reference {schemaKey}.", type.FullName, schemaKey);
                return new OpenApiSchemaReference(schemaKey);
            }
            return schema;
        }

        private static bool IsDictionary(Type type)
        {
            // Check if type is Dictionary<,> or IDictionary<,>
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(Dictionary<,>) || genericDef == typeof(IDictionary<,>))
                    return true;
            }

            // Check if type implements IDictionary<,> through inheritance
            return type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
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
            // Check if it's an array
            if (type.IsArray)
                return true;

            // Check if the type itself is a generic IEnumerable<> (for interface types like IList<string>)
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return true;

            // Use GetInterfaces() to detect ALL collection types including custom classes inheriting from List<T>, Collection<T>, etc.
            // GetInterfaces() automatically traverses the inheritance hierarchy
            return type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private static Type? GetElementType(Type type)
        {
            // Handle arrays
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            // For non-array types, find the IEnumerable<T> interface and extract T
            // This correctly handles CustomList : List<string> where GetGenericArguments() on the type itself may not return the element type
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                return enumerableInterface.GetGenericArguments().FirstOrDefault();
            }

            // Fallback: if the type itself is a generic type, try to get the first generic argument
            if (type.IsGenericType && type.GetGenericArguments().Length > 0)
            {
                return type.GetGenericArguments().FirstOrDefault();
            }

            // Fallback: recurse into base type
            if (type.BaseType != null)
            {
                return GetElementType(type.BaseType);
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