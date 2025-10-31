using Microsoft.OpenApi;
using Namotion.Reflection;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultDiscoveryService : IDiscoveryService
    {
        public List<Assembly> GetAllAssemblies()
        {
            //Find all of the dll's that are in the 'bin' directory with the current app, even if they're not loaded yet
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dll);
                    if (!assemblies.Any(a => a.FullName == assemblyName.FullName))
                    {
                        var assembly = Assembly.Load(assemblyName);
                        assemblies.Add(assembly);
                    }
                }
                catch
                {
                    // Ignore any DLLs that cannot be loaded as assemblies
                }
            }
            return assemblies;
        }

        public List<Assembly> FindCanonicalAssemblies() =>
            GetAllAssemblies()
                .Where(assembly => assembly.GetReferencedAssemblies()
                    .Any(referencedAssembly => referencedAssembly.Name == "Zen.CanonicaLib.DataAnnotations"))
                .Where(assembly => !assembly.FullName!.StartsWith("Zen.CanonicaLib"))
                .ToList();

        public Assembly? FindCanonicalAssembly(string assemblyName) =>
            GetAllAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name.ToLowerInvariant() == assemblyName.ToLowerInvariant() &&
                    assembly.GetReferencedAssemblies()
                        .Any(referencedAssembly => referencedAssembly.Name == "Zen.CanonicaLib.DataAnnotations"));

        public IList<Type> FindControllerDefinitions(Assembly assembly) =>
            assembly.GetTypes()
                .Where(type => type.IsInterface && type.GetCustomAttributes(typeof(OpenApiPathAttribute), inherit: false).Any())
                .ToList();

        public IList<Type> FindWebhookDefinitions(Assembly assembly) =>
            assembly.GetTypes()
                .Where(type => type.IsInterface && type.GetCustomAttributes(typeof(OpenApiWebhookAttribute), inherit: false).Any())
                .ToList();

        public IList<MethodInfo> FindEndpointDefinitions(Type controllerDefinition) => controllerDefinition.GetMethods()
                .Where(method => method.GetCustomAttributes(typeof(OpenApiEndpointAttribute), inherit: false).Any())
                .ToList();

        public IList<Type> FindSchemaDefinitions(Assembly assembly)
        {
            var schemaTypes = assembly.GetTypes()
                .Where(type => type.IsClass || type.IsEnum || type.IsValueType)
                .ToList();

            var controllerDefinitions = FindControllerDefinitions(assembly);
            foreach (var controllerDefinition in controllerDefinitions)
            {
                var endpointDefinitions = FindEndpointDefinitions(controllerDefinition);
                foreach (var endpointDefinition in endpointDefinitions)
                {
                    var responseAttributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
                    foreach (var responseAttribute in responseAttributes)
                    {
                        var schemaType = responseAttribute.Type;
                        if (schemaType != null && !schemaTypes.Contains(schemaType))
                        {
                            schemaTypes.Add(schemaType);
                        }
                    }
                }
            }

            return schemaTypes;
        }

        public ISet<OpenApiTag> FindControllerTags(Assembly assembly)
        {
            var tagAttributes = FindControllerDefinitions(assembly)
                  .Select(cd => new { TagAttribute = cd.GetCustomAttribute<OpenApiTagAttribute>(), Summary = cd.GetXmlDocsSummary() })
                  .Where(tag => tag.TagAttribute != null)
                  .ToList();

            return tagAttributes.Select(x => new OpenApiTag()
            {
                Name = x.TagAttribute!.Tag,
                Description = x.Summary.IfEmpty(null)
            }).ToHashSet();
        }


        public ILibrary GetLibraryInstance(Assembly assembly)
        {
            var libraries = assembly.GetTypes()
                .Where(t => typeof(ILibrary).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (!libraries.Any())
                throw new InvalidOperationException("No implementation of ILibrary found in the target assembly.");

            if (libraries.Count() > 1)
                throw new InvalidOperationException("Multiple implementations of ILibrary found in the target assembly.");

            var library = libraries.First();

            if (library == null)
                throw new InvalidOperationException("Failed to instantiate ILibrary implementation.");

            ILibrary libraryInstance = (ILibrary)Activator.CreateInstance(library)!;

            return libraryInstance;
        }

        public IList<string> GetDocumentList(Assembly assembly)
        {
            // search embedded resources for markdown documents
            var documentNames = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                .Select(name => name.Replace($"{assembly.FullName.Split(",")[0]}.Docs.", ""))
                .ToList();

            return documentNames;
        }

        public bool HasIndexDocument(Assembly assembly)
        {
            var documentNames = GetDocumentList(assembly);
            return documentNames.Contains("index.md", StringComparer.OrdinalIgnoreCase);
        }

        public string GetDocumentContent(Assembly assembly, string documentName)
        {
            var resourceName = $"{assembly.FullName.Split(",")[0]}.Docs.{documentName}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Document '{documentName}' not found in assembly resources.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public ISet<OpenApiTag> FindWebhookTags(Assembly assembly)
        {
            var tagAttributes = FindWebhookDefinitions(assembly)
                  .Select(wd => new { TagAttribute = wd.GetCustomAttribute<OpenApiTagAttribute>(), Summary = wd.GetXmlDocsSummary() })
                  .Where(tag => tag.TagAttribute != null)
                  .ToList();

            return tagAttributes.Select(x => new OpenApiTag()
            {
                Name = x.TagAttribute!.Tag,
                Description = x.Summary.IfEmpty(null)
            }).ToHashSet();
        }
    }
}
