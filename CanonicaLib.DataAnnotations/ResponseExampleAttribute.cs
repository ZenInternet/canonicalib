using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ResponseExampleAttribute : ExampleAttribute
    {
        public int StatusCode { get; set; }

        public ResponseExampleAttribute(int statusCode, Type exampleType)
            : base(exampleType)
        {
            StatusCode = statusCode;
        }
    }
}
