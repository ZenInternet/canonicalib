using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class OpenApiPathAttribute : Attribute
    {
        public string? PathPattern { get; }

        public OpenApiPathAttribute(string? pathPattern)
        {
            PathPattern = pathPattern;
        }
    }
}
