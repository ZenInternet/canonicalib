using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class OpenApiTagAttribute : Attribute
    {
        public string Tag { get; set; }
        public OpenApiTagAttribute(string tag)
        {
            Tag = tag;
        }
    }
}
