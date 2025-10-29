using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OpenApiEndpointAttribute : OpenApiPathAttribute
    {
        public string HttpMethod { get; }
        public OpenApiEndpointAttribute(string pathPattern, string httpMethod)
            : base(pathPattern)
        {
            HttpMethod = httpMethod;
        }
    }
}
