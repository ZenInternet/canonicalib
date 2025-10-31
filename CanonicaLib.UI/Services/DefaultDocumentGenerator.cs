
using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultDocumentGenerator : IDocumentGenerator
    {
        private readonly IInfoGenerator InfoGenerator;
        private readonly IPathsGenerator PathsGenerator;
        private readonly IComponentsGenerator ComponentsGenerator;
        private readonly ITagGroupsGenerator TagGroupsGenerator;
        private readonly IWebhooksGenerator WebhooksGenerator;
        private readonly IServersGenerator ServersGenerator;

        public DefaultDocumentGenerator(
            IInfoGenerator infoGenerator,
            IServersGenerator serversGenerator,
            IPathsGenerator pathsGenerator,
            IComponentsGenerator componentsGenerator,
            ITagGroupsGenerator tagGroupsGenerator,
            IWebhooksGenerator webhooksGenerator)
        {
            InfoGenerator = infoGenerator;
            ServersGenerator = serversGenerator;
            PathsGenerator = pathsGenerator;
            ComponentsGenerator = componentsGenerator;
            TagGroupsGenerator = tagGroupsGenerator;
            WebhooksGenerator = webhooksGenerator;
            WebhooksGenerator = webhooksGenerator;
        }

        public OpenApiDocument GenerateDocument(Assembly assembly)
        {

            var generatorContext = new GeneratorContext(assembly);

            InfoGenerator.GenerateInfo(generatorContext);
            ServersGenerator.GenerateServers(generatorContext);
            ComponentsGenerator.GenerateComponents(generatorContext);
            TagGroupsGenerator.GenerateTagGroups(generatorContext);
            PathsGenerator.GeneratePaths(generatorContext);
            WebhooksGenerator.GenerateWebhooks(generatorContext);
            WebhooksGenerator.GenerateWebhooks(generatorContext);

            return generatorContext.Document;
        }
    }

}
