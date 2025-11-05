using Microsoft.OpenApi;
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

        public bool AddSchema(Type type, IOpenApiSchema schema, AssemblyReferenceType referenceType)
        {
            var schemaKey = type.FullName ?? type.Name;
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
            var schemaKey = type.FullName ?? type.Name;
            if (Document.Components!.Schemas!.ContainsKey(schemaKey))
                return new OpenApiSchemaReference(schemaKey);
            return null;
        }
    }
}
