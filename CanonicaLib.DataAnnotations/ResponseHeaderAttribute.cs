using Microsoft.AspNetCore.Mvc;
using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ResponseHeaderAttribute : Attribute
    {
        public Type Type { get; set; }

        public int StatusCode { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public ResponseHeaderAttribute(Type? type, int statusCode, string name, string description)
        {
            Type = type;
            StatusCode = statusCode;
            Name = name;
            Description = description;
        }

        public ResponseHeaderAttribute(Type? type, int statusCode, string name)
        {
            Type = type;
            StatusCode = statusCode;
            Name = name;
            Description = null;
        }
    }
}
