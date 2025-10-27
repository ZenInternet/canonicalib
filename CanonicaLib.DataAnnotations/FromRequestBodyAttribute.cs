using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromRequestBodyAttribute : Attribute
    {
    }
}
