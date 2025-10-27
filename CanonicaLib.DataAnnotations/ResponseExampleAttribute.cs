using System;

namespace CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ResponseExampleAttribute : Attribute
    {
        public int StatusCode { get; set; }
        public Type ExampleType { get; set; }

        public ResponseExampleAttribute(int statusCode, Type exampleType)
        {
            StatusCode = statusCode;
            ExampleType = exampleType;
        }
    }
}
