using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class PathAttribute : Attribute
    {
        public string PathPattern { get; }

        public PathAttribute(string pathPattern)
        {
            PathPattern = pathPattern;
        }
    }
}
