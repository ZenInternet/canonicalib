using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface, 
                    Inherited = true, AllowMultiple = false)]
    public class OpenApiExcludedTypeAttribute : Attribute
    {
    }
}
