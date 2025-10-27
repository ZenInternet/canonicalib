using Zen.CanonicaLib.DataAnnotations;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class DiscoveryService
    {
        public List<Assembly> FindCanonicalAssemblies() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetReferencedAssemblies()
                    .Any(referencedAssembly => referencedAssembly.Name == "Zen.CanonicaLib.DataAnnotations"))
                .Where(assembly => !assembly.FullName!.StartsWith("Zen.CanonicaLib"))
                .ToList();

        public Assembly? FindCanonicalAssembly(string assemblyName) => 
            AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name.ToLowerInvariant() == assemblyName.ToLowerInvariant() &&
                    assembly.GetReferencedAssemblies()
                        .Any(referencedAssembly => referencedAssembly.Name == "Zen.CanonicaLib.DataAnnotations"));

        public IList<Type> FindControllerDefinitions(Assembly assembly) =>
            assembly.GetTypes()
                .Where(type => type.IsInterface && type.GetCustomAttributes(typeof(PathAttribute), inherit: false).Any())
                .ToList();

        internal IList<MethodInfo> FindEndpointDefinitions(Type controllerDefinition) => controllerDefinition.GetMethods()
                .Where(method => method.GetCustomAttributes(typeof(EndpointAttribute), inherit: false).Any())
                .ToList();

        internal IList<Type> FindSchemaDefinitions(Assembly assembly)
        {
            var schemaTypes = new List<Type>();

            var controllerDefinitions = FindControllerDefinitions(assembly);
            foreach (var controllerDefinition in controllerDefinitions)
            {
                var endpointDefinitions = FindEndpointDefinitions(controllerDefinition);
                foreach (var endpointDefinition in endpointDefinitions)
                {
                    var responseAttributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
                    foreach (var responseAttribute in responseAttributes)
                    {
                        var schemaType = responseAttribute.ResponseType;
                        if (schemaType != null && !schemaTypes.Contains(schemaType))
                        {
                            schemaTypes.Add(schemaType);
                        }
                    }
                }
            }

            return schemaTypes;
        }
    }
}
