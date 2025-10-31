using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Specifies OpenAPI parameter metadata for method parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// public IActionResult GetUser([OpenApiParameter("userId", "path", true)] int id)
    /// {
    ///     // method implementation
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class OpenApiParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the parameter in the OpenAPI specification.
        /// </summary>
        /// <value>The parameter name.</value>
        public string? Name { get; }

        /// <summary>
        /// Gets the location of the parameter (e.g., "query", "header", "path", "cookie").
        /// </summary>
        /// <value>The parameter location.</value>
        public string In { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is required.
        /// </summary>
        /// <value><c>true</c> if the parameter is required; otherwise, <c>false</c>.</value>
        public bool Required { get; }

        /// <summary>
        /// Gets or sets an optional description for the parameter.
        /// </summary>
        /// <value>A description of the parameter's purpose.</value>
        public string? Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiParameterAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter in the OpenAPI specification.</param>
        /// <param name="in">The location of the parameter.</param>
        /// <param name="description">An optional description of the parameter.</param>
        /// <param name="required">Whether the parameter is required.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="in"/> is null, empty, or whitespace.</exception>
        public OpenApiParameterAttribute(string? name, string @in, string? description = null, bool required = false)
        {            
            if (string.IsNullOrWhiteSpace(@in))
                throw new ArgumentException("Parameter location cannot be null, empty, or whitespace.", nameof(@in));

            Name = name;
            In = @in;
            Description = description;
            Required = required;
        }
    }
}