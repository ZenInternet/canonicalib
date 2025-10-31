using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IDiscoveryService
    {
        List<Assembly> GetAllAssemblies();
        List<Assembly> FindCanonicalAssemblies();
        Assembly? FindCanonicalAssembly(string assemblyName);
        IList<Type> FindControllerDefinitions(Assembly assembly);
        IList<Type> FindWebhookDefinitions(Assembly assembly);
        public ISet<OpenApiTag> FindControllerTags(Assembly assembly);
        public ISet<OpenApiTag> FindWebhookTags(Assembly assembly);
        IList<MethodInfo> FindEndpointDefinitions(Type controllerDefinition);
        IList<Type> FindSchemaDefinitions(Assembly assembly);
        public ILibrary GetLibraryInstance(Assembly assembly);
        public IService? GetServiceInstance(Assembly assembly);
        public bool HasIndexDocument(Assembly assembly);
        public IList<string> GetDocumentList(Assembly assembly);
        public string GetDocumentContent(Assembly assembly, string documentName);
    }
}