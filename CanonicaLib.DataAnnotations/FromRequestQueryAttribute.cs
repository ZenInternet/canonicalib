using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class FromRequestQueryAttribute : OpenApiParameterAttribute
    {
        public FromRequestQueryAttribute(string name, string? description = null, bool required = false)
            : base(name, @in: "query", description, required)
        {
        }
    }
}
