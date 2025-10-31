using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class FromRequestPathAttribute : OpenApiParameterAttribute
    {
        public FromRequestPathAttribute(string? name = null, string? description = null, bool required = false)
            : base(name, @in: "path", description, required)
        {
        }
    }
}
