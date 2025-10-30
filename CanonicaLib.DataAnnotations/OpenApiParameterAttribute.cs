using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class OpenApiParameterAttribute : Attribute
    {
        public string Name { get; }
        public string In { get; }
        public bool Required { get; }

        public OpenApiParameterAttribute(string name, string @in, bool required = false)
        {
            Name = name;
            In = @in;
            Required = required;
        }
    }
}