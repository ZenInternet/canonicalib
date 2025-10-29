
using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class DocumentGenerator
    {
        private readonly InfoGenerator InfoGenerator;
        private readonly PathsGenerator PathsGenerator;
        private readonly ComponentsGenerator ComponentsGenerator;
        private readonly TagGroupsGenerator TagGroupsGenerator;

        public DocumentGenerator(
            InfoGenerator infoGenerator,
            PathsGenerator pathsGenerator,
            ComponentsGenerator componentsGenerator,
            TagGroupsGenerator tagGroupsGenerator)
        {
            InfoGenerator = infoGenerator;
            PathsGenerator = pathsGenerator;
            ComponentsGenerator = componentsGenerator;
            TagGroupsGenerator = tagGroupsGenerator;
        }

        public OpenApiDocument GenerateDocument(Assembly assembly, DiscoveryService discoveryService)
        {

            var generatorContext = new GeneratorContext(assembly, discoveryService);

            InfoGenerator.GenerateInfo(generatorContext);
            PathsGenerator.GeneratePaths(generatorContext);
            ComponentsGenerator.GenerateComponents(generatorContext);
            TagGroupsGenerator.GenerateTagGroups(generatorContext);

            return generatorContext.Document;
        }
    }

}
