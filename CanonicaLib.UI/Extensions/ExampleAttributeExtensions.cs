using Namotion.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Extensions
{
    public static class ExampleAttributeExtensions
    {

        public static object GetExample(this ExampleAttribute exampleAttribute)
        {
            var example = Activator.CreateInstance(exampleAttribute.ExampleType);

            bool implementsIExample = example?.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExample<>)) ?? false;

            if (!implementsIExample)
            {
                throw new InvalidOperationException($"Instance of type {exampleAttribute.ExampleType.FullName} does not implement IExample<T>");
            }

            var exampleInterface = example?.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExample<>))!;
            var exampleProperty = exampleInterface.GetProperty("Example");
            var typedExample = exampleProperty!.GetValue(example);

            return typedExample!;
        }

        public static string GetName(this ExampleAttribute exampleAttribute)
        {
            var summary = exampleAttribute.ExampleType.GetXmlDocsSummary();

            var example = Activator.CreateInstance(exampleAttribute.ExampleType);
            var nameProperty = exampleAttribute.ExampleType.GetProperty("Name");
            var name = nameProperty?.GetValue(example);

            return (string?)name ?? summary ?? string.Empty;
        }

        public static string GetDescription(this ExampleAttribute exampleAttribute)
        {
            var remarks = exampleAttribute.ExampleType.GetXmlDocsRemarks();
            return remarks ?? string.Empty;
        }

        /// <summary>
        /// Validates an example with enhanced validation context and service provider support
        /// </summary>
        public static object GetExampleContent(this ExampleAttribute exampleAttribute, out IList<ValidationResult> validationResults, IServiceProvider? serviceProvider = null, IDictionary<object, object?>? items = null)
        {
            var typedExample = exampleAttribute.GetExample();

            IDictionary<object, object?> validationItems = items ?? new Dictionary<object, object?>();

            var validationContext = new ValidationContext(typedExample, serviceProvider: serviceProvider, validationItems);
            var results = new List<ValidationResult>();

            // Perform recursive validation
            ValidateObjectRecursively(typedExample, validationContext, results, new HashSet<object>(), serviceProvider);

            validationResults = results;

            return typedExample;
        }

        /// <summary>
        /// Recursively validates an object and all its properties, including items in enumerable properties
        /// </summary>
        /// <param name="obj">The object to validate</param>
        /// <param name="validationContext">The validation context</param>
        /// <param name="validationResults">Collection to store validation results</param>
        /// <param name="validatedObjects">Set to track already validated objects to prevent infinite recursion</param>
        /// <param name="serviceProvider">Optional service provider for dependency injection during validation</param>
        /// <returns>True if the object and all nested objects are valid, false otherwise</returns>
        private static bool ValidateObjectRecursively(object? obj, ValidationContext validationContext, List<ValidationResult> validationResults, HashSet<object> validatedObjects, IServiceProvider? serviceProvider = null)
        {
            if (obj == null)
                return true;

            // Prevent infinite recursion by tracking validated objects
            if (validatedObjects.Contains(obj))
                return true;

            // Skip validation for primitive types and strings
            if (IsPrimitiveType(obj.GetType()))
                return true;

            validatedObjects.Add(obj);

            bool isValid = true;
            var objectType = obj.GetType();

            try
            {
                // Create a new validation context for this object
                var currentValidationContext = new ValidationContext(obj, serviceProvider, validationContext.Items)
                {
                    MemberName = validationContext.MemberName
                };

                // Perform standard validation on the current object
                var currentResults = new List<ValidationResult>();
                bool currentObjectValid = Validator.TryValidateObject(obj, currentValidationContext, currentResults, validateAllProperties: true);

                if (!currentObjectValid)
                {
                    validationResults.AddRange(currentResults);
                    isValid = false;
                }

                // Check for IValidatableObject implementation for custom validation
                if (obj is IValidatableObject validatableObject)
                {
                    var customValidationResults = validatableObject.Validate(currentValidationContext);
                    var customResults = customValidationResults.ToList();
                    if (customResults.Any())
                    {
                        validationResults.AddRange(customResults);
                        isValid = false;
                    }
                }

                // Validate all properties recursively
                var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetGetMethod()?.IsPublic == true);

                foreach (var property in properties)
                {
                    // Skip indexed properties (indexers) - we'll handle them separately if the object implements IEnumerable
                    if (IsIndexedProperty(property))
                        continue;

                    var propertyValue = property.GetValue(obj);

                    if (propertyValue == null)
                        continue;

                    var propertyType = property.PropertyType;

                    // Handle enumerable properties (collections, arrays, lists, etc.)
                    if (IsEnumerableType(propertyType) && !IsPrimitiveType(propertyType))
                    {
                        if (propertyValue is IEnumerable enumerable)
                        {
                            int index = 0;
                            foreach (var item in enumerable)
                            {
                                if (item != null)
                                {
                                    var itemValidationContext = new ValidationContext(item, serviceProvider, validationContext.Items)
                                    {
                                        MemberName = $"{property.Name}[{index}]"
                                    };

                                    if (!ValidateObjectRecursively(item, itemValidationContext, validationResults, validatedObjects, serviceProvider))
                                    {
                                        isValid = false;
                                    }
                                }
                                index++;
                            }
                        }
                    }
                    // Handle complex object properties
                    else if (!IsPrimitiveType(propertyType))
                    {
                        var propertyValidationContext = new ValidationContext(propertyValue, serviceProvider, validationContext.Items)
                        {
                            MemberName = property.Name
                        };

                        if (!ValidateObjectRecursively(propertyValue, propertyValidationContext, validationResults, validatedObjects, serviceProvider))
                        {
                            isValid = false;
                        }
                    }
                }

                // Handle indexed properties by validating items if the object implements IEnumerable
                if (obj is IEnumerable enumerable && !IsPrimitiveType(objectType))
                {
                    int index = 0;
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                        {
                            var itemValidationContext = new ValidationContext(item, serviceProvider, validationContext.Items)
                            {
                                MemberName = $"[{index}]"
                            };

                            if (!ValidateObjectRecursively(item, itemValidationContext, validationResults, validatedObjects, serviceProvider))
                            {
                                isValid = false;
                            }
                        }
                        index++;
                    }
                }
            }
            finally
            {
                validatedObjects.Remove(obj);
            }

            return isValid;
        }

        /// <summary>
        /// Determines if a property is an indexed property (indexer)
        /// </summary>
        private static bool IsIndexedProperty(PropertyInfo property)
        {
            return property.GetIndexParameters().Length > 0;
        }

        /// <summary>
        /// Determines if a type is a primitive type (including string, DateTime, etc.)
        /// </summary>
        private static bool IsPrimitiveType(Type type)
        {
            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
            }

            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        /// <summary>
        /// Determines if a type implements IEnumerable (but is not a string)
        /// </summary>
        private static bool IsEnumerableType(Type type)
        {
            // String implements IEnumerable but we don't want to treat it as a collection
            if (type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}