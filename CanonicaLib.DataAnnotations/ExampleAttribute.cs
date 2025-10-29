using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class ExampleAttribute : Attribute
    {
        public Type ExampleType { get; set; }

        public ExampleAttribute(Type exampleType)
        {
            ExampleType = exampleType;
        }
    }
}
