using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ResponseAttribute : Attribute
    {
        public int StatusCode { get; }

        public string? Description { get; }

        public Type? ResponseType { get; set; }

        public ResponseAttribute(int statusCode, string? description = null, Type? responseType = null)
        {
            StatusCode = statusCode;
            Description = description;
            ResponseType = responseType;
        }
    }
}
