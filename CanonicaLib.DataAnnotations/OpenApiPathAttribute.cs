using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class OpenApiPathAttribute : OpenApiExcludedTypeAttribute
    {
        public string? PathPattern { get; }

        public OpenApiPathAttribute(string? pathPattern)
        {
            PathPattern = pathPattern;
        }
    }
}
