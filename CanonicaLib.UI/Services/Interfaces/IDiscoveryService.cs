using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public enum AssemblyReferenceType
    {
        Excluded,
        External,
        Internal,
    }

    public interface IDiscoveryService
    {
        public List<Assembly> GetAllAssemblies();
        public List<Assembly> FindCanonicalAssemblies();
        public Assembly? FindCanonicalAssembly(string assemblyName);
        public IList<Type> FindControllerDefinitions(Assembly assembly);
        public IList<Type> FindWebhookDefinitions(Assembly assembly);
        public ISet<OpenApiTag> FindControllerTags(Assembly assembly);
        public ISet<OpenApiTag> FindWebhookTags(Assembly assembly);
        public IList<MethodInfo> FindEndpointDefinitions(Type controllerDefinition);
        public IList<Type> FindSchemaDefinitions(Assembly assembly);
        public AssemblyReferenceType GetAssemblyReferenceType(Assembly assembly, Type type);
        public ILibrary GetLibraryInstance(Assembly assembly);
        public IService? GetServiceInstance(Assembly assembly);
        public ISecureService? GetSecureServiceInstance(Assembly assembly);
        public bool HasIndexDocument(Assembly assembly);
        public IList<string> GetDocumentList(Assembly assembly);
        public string GetDocumentContent(Assembly assembly, string documentName);
    }
}