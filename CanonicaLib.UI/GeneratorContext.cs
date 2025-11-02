using Microsoft.OpenApi;
using System.Reflection;

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

        /// <summary>
        /// Gets the collection of schemas discovered during generation.
        /// </summary>
        /// <value>A dictionary mapping schema names to their OpenAPI schema definitions.</value>
        /// <remarks>
        /// This collection is populated by schema generators and can be referenced
        /// by other generators to avoid duplication and ensure consistency.
        /// </remarks>
        public IDictionary<string, IOpenApiSchema> Schemas { get; } = new Dictionary<string, IOpenApiSchema>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorContext"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to generate documentation for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
        public GeneratorContext(Assembly assembly)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }
    }
}
