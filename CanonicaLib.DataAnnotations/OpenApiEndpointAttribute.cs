using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Specifies the HTTP method and optional path pattern for an OpenAPI endpoint operation.
    /// This attribute extends <see cref="OpenApiPathAttribute"/> to include HTTP method information.
    /// </summary>
    /// <remarks>
    /// This attribute is used to define RESTful API endpoints with specific HTTP methods.
    /// It can be applied to interface methods to specify both the path pattern and HTTP method
    /// for OpenAPI documentation generation.
    /// </remarks>
    /// <example>
    /// <code>
    /// public interface IUsersController
    /// {
    ///     [OpenApiEndpoint("/users/{id}", "GET")]
    ///     Task&lt;User&gt; GetUser(int id);
    ///     
    ///     [OpenApiEndpoint("POST")]
    ///     Task&lt;User&gt; CreateUser(User user);
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OpenApiEndpointAttribute : OpenApiPathAttribute
    {
        /// <summary>
        /// Gets the HTTP method for this endpoint operation.
        /// </summary>
        /// <value>The HTTP method (e.g., GET, POST, PUT, DELETE, PATCH, etc.).</value>
        /// <remarks>
        /// Common HTTP methods are available as constants in the <see cref="Methods"/> class.
        /// </remarks>
        public string HttpMethod { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiEndpointAttribute"/> class
        /// with the specified path pattern and HTTP method.
        /// </summary>
        /// <param name="pathPattern">The path pattern for the endpoint (e.g., "/users/{id}").</param>
        /// <param name="httpMethod">The HTTP method for the endpoint operation.</param>
        /// <remarks>
        /// The path pattern can include route parameters using curly braces (e.g., "{id}").
        /// Use constants from the <see cref="Methods"/> class for standard HTTP methods.
        /// </remarks>
        public OpenApiEndpointAttribute(string pathPattern, string httpMethod)
            : base(pathPattern)
        {
            HttpMethod = httpMethod;
        }
    }
}
