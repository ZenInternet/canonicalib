using Microsoft.OpenApi;
using System.Linq;
using System.Reflection;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI
{
    /// <summary>
    /// Provides context and shared state for OpenAPI document generation.
    /// </summary>
    /// <remarks>
    /// This class maintains the state during the document generation process,
    /// including the target assembly, the OpenAPI document being built, and
    /// collected schemas that can be referenced across generators.
    /// </remarks>
    public sealed class GeneratorContext
    {
        /// <summary>
        /// Gets the assembly being processed for OpenAPI document generation.
        /// </summary>
        /// <value>The target assembly containing the types to document.</value>
        public Assembly Assembly { get; init; }

        public OpenApiDocument Document { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorContext"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to generate documentation for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
        public GeneratorContext(Assembly assembly)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            var document = new OpenApiDocument();
            document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            document.Components ??= new OpenApiComponents() ;
            document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
            Document = document;
        }

        /// <summary>
        /// Builds a stable, identifier-safe schema key for a type.
        /// </summary>
        /// <remarks>
        /// For constructed generic types this avoids <see cref="Type.FullName"/> — which is the
        /// assembly-qualified name containing backticks, brackets, commas and spaces (e.g.
        /// <c>Zen.Contract.PagedResult`1[[...ConnectivityServiceResponse, ..., Version=..., Culture=...,
        /// PublicKeyToken=null]]</c>) — and instead produces a clean name such as
        /// <c>Zen.Contract.PagedResultOfZen.Contract.Services.Inventory.Capability.ConnectivityServiceResponse</c>.
        /// Without this, downstream OpenAPI code generators emit invalid identifiers (the surviving
        /// spaces produce uncompilable TypeScript). Non-generic types keep their existing FullName key.
        /// </remarks>
        public static string GetSchemaKey(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                type = underlying;
            }

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                var baseName = definition.FullName ?? definition.Name;
                var arityMarker = baseName.IndexOf('`');
                if (arityMarker >= 0)
                {
                    baseName = baseName.Substring(0, arityMarker);
                }

                var arguments = type.GetGenericArguments().Select(GetSchemaKey);
                return $"{baseName}Of{string.Join("And", arguments)}";
            }

            return type.FullName ?? type.Name;
        }

        public bool AddSchema(Type type, IOpenApiSchema schema, AssemblyReferenceType referenceType)
        {
            var schemaKey = GetSchemaKey(type);
            if (referenceType == AssemblyReferenceType.Internal || referenceType == AssemblyReferenceType.External)
            {
                if (Document.Components!.Schemas!.ContainsKey(schemaKey))
                {
                    return true;
                }
                
                Document.Components!.Schemas![schemaKey] = schema;
                return true;
            }
            return false;
        }

        public IOpenApiSchema? GetExistingSchema(Type type)
        {
            var schemaKey = GetSchemaKey(type);
            if (Document.Components!.Schemas!.ContainsKey(schemaKey))
                return new OpenApiSchemaReference(schemaKey);
            return null;
        }
    }
}
