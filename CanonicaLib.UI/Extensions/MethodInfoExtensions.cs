using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Extensions
{
    internal static class MethodInfoExtensions
    {
        /// <summary>
        /// Gets the <see cref="OpenApiEndpointAttribute"/> for a method, searching
        /// parent interfaces if not found directly. This handles the case where a
        /// child interface uses <c>new</c> to redeclare a method (e.g. to add
        /// parameter-level attributes) without repeating the endpoint attribute.
        /// </summary>
        public static OpenApiEndpointAttribute? GetEndpointAttribute(this MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<OpenApiEndpointAttribute>();
            if (attribute != null)
                return attribute;

            var declaringType = method.DeclaringType;
            if (declaringType == null)
                return null;

            foreach (var parentInterface in declaringType.GetInterfaces())
            {
                var parentMethod = parentInterface.GetMethod(method.Name);
                if (parentMethod != null)
                {
                    attribute = parentMethod.GetCustomAttribute<OpenApiEndpointAttribute>();
                    if (attribute != null)
                        return attribute;
                }
            }

            return null;
        }
    }
}
