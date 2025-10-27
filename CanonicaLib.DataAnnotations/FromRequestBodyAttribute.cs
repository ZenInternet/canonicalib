using System;
namespace CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromRequestBodyAttribute : Attribute
    {
    }
}
