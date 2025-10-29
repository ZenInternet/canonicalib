using Microsoft.AspNetCore.Mvc;
using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ResponseAttribute : ProducesResponseTypeAttribute
    {
        public string? Description { get; set; }

        public ResponseAttribute(Type? type, int statusCode, string description)
            : base(type ?? typeof(void), statusCode)
        {
            Description = description;
        }

        public ResponseAttribute(int statusCode, string description)
            : base(typeof(void), statusCode)
        {
            Description = description;
        }

    }
}
