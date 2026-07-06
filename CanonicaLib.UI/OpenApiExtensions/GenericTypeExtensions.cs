using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.OpenApiExtensions
{
    /// <summary>
    /// Vendor extension (<c>x-generic-type-parameters</c>) stamped on the schema of an open generic
    /// type definition (e.g. <c>PagedResult&lt;T&gt;</c>). OpenAPI cannot express generics, so the
    /// spec collapses the type to a concrete schema; this extension records the original type
    /// parameter names so a downstream code generator can faithfully reconstruct the generic in the
    /// target language. Emitted as a JSON string array, e.g. <c>["T"]</c>.
    /// </summary>
    public class GenericTypeParametersExtension : IOpenApiExtension
    {
        private readonly IList<string> _parameters;

        public GenericTypeParametersExtension(IList<string> parameters)
        {
            _parameters = parameters;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var parameter in _parameters)
            {
                writer.WriteValue(parameter);
            }
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Vendor extension stamped on a property schema whose type is (or whose collection element is) an
    /// unbound generic type parameter of the declaring type. Records the parameter name so a downstream
    /// generator can rewrite the property to the generic parameter rather than the erased
    /// <c>object</c>/<c>any</c> the OpenAPI spec necessarily carries. Used with key
    /// <c>x-generic-ref</c> when the property type IS the parameter, and <c>x-generic-item-ref</c> when
    /// the property is a collection whose element is the parameter. Emitted as a JSON string, e.g. <c>"T"</c>.
    /// </summary>
    public class GenericParameterRefExtension : IOpenApiExtension
    {
        private readonly string _parameterName;

        public GenericParameterRefExtension(string parameterName)
        {
            _parameterName = parameterName;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteValue(_parameterName);
        }
    }
}
