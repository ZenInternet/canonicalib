using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class FromRequestHeaderAttribute : OpenApiParameterAttribute
    {
        public FromRequestHeaderAttribute(string name, string? description = null, bool required = false)
            : base(name, @in: "header", description, required)
        {
        }
    }
}
