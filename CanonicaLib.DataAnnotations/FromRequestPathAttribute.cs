using System;
namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class FromRequestPathAttribute : Attribute
    {
    }
}
