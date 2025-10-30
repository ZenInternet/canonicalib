using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class FromRequestPathAttribute : OpenApiParameterAttribute
    {
        public FromRequestPathAttribute(string? name = null, bool required = false)
            : base(name, @in: "path", required)
        {
        }
    }
}
