using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Specifies the OpenAPI tag to be used for grouping operations in the generated documentation.
    /// </summary>
    /// <example>
    /// <code>
    /// [OpenApiTag("Users")]
    /// public class UserController : ApiController
    /// {
    ///     // controller implementation
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface, 
                    Inherited = false, AllowMultiple = false)]
    public sealed class OpenApiTagAttribute : Attribute
    {
        /// <summary>
        /// Gets the tag name used for OpenAPI documentation grouping.
        /// </summary>
        /// <value>The tag name for OpenAPI documentation.</value>
        public string Tag { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiTagAttribute"/> class.
        /// </summary>
        /// <param name="tag">The tag name for OpenAPI documentation grouping.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is null, empty, or whitespace.</exception>
        public OpenApiTagAttribute(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag cannot be null, empty, or whitespace.", nameof(tag));
            
            Tag = tag;
        }
    }
}
