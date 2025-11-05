using Microsoft.OpenApi;
using System.ComponentModel.DataAnnotations;

namespace Zen.CanonicaLib.UI.OpenApiExtensions
{
    internal class ValidationResultsExtension : IOpenApiExtension
    {
        private readonly IList<ValidationResult> _validationResults;
        public ValidationResultsExtension(IList<ValidationResult> validationResults)
        {
            _validationResults = validationResults;
        }
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var result in _validationResults)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("MemberNames");
                writer.WriteStartArray();
                foreach (var memberName in result.MemberNames)
                {
                    writer.WriteValue(memberName);
                }
                writer.WriteEndArray();
                writer.WriteProperty("ErrorMessage", result.ErrorMessage);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}