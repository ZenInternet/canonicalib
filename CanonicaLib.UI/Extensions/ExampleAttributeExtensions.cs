using Namotion.Reflection;
using System.ComponentModel.DataAnnotations;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Extensions
{
    public static class ExampleAttributeExtensions
    {

        public static object GetExample(this ExampleAttribute exampleAttribute)
        {
            var example = Activator.CreateInstance(exampleAttribute.ExampleType);

            bool implementsIExample = example.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExample<>));

            if (!implementsIExample)
            {
                throw new InvalidOperationException($"Instance of type {exampleAttribute.ExampleType.FullName} does not implement IExample<T>");
            }

            var exampleInterface = example.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExample<>));
            var exampleProperty = exampleInterface.GetProperty("Example");
            var typedExample = exampleProperty.GetValue(example);

            return typedExample;
        }

        public static string GetName(this ExampleAttribute exampleAttribute)
        {
            var summary = exampleAttribute.ExampleType.GetXmlDocsSummary();

            var example = Activator.CreateInstance(exampleAttribute.ExampleType);
            var nameProperty = exampleAttribute.ExampleType.GetProperty("Name");
            var name = nameProperty.GetValue(example);

            return (string?)name ?? summary ?? string.Empty;
        }

        public static string GetDescription(this ExampleAttribute exampleAttribute)
        {
            var remarks = exampleAttribute.ExampleType.GetXmlDocsRemarks();
            return remarks ?? string.Empty;
        }

        public static object GetExampleContent(this ExampleAttribute exampleAttribute)
        {
            var typedExample = exampleAttribute.GetExample();

            // TODO - perform validation on typedExample here using the data annotations defined on the type T
            var validationContext = new ValidationContext(typedExample, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(typedExample, validationContext, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                throw new InvalidOperationException($"Example of type {exampleAttribute.ExampleType.FullName} is not valid: {errors}");
            }

            return typedExample;
        }
    }
}
