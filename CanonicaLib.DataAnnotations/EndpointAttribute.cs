using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class EndpointAttribute : PathAttribute
    {
        public string HttpMethod { get; }
        public EndpointAttribute(string pathPattern, string httpMethod)
            : base(pathPattern)
        {
            HttpMethod = httpMethod;
        }
    }
}
