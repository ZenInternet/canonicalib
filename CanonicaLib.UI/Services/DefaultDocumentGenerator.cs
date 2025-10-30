
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

        public DefaultDocumentGenerator(
            IInfoGenerator infoGenerator,
            IPathsGenerator pathsGenerator,
            IComponentsGenerator componentsGenerator,
            ITagGroupsGenerator tagGroupsGenerator)
        {
            InfoGenerator = infoGenerator;
            PathsGenerator = pathsGenerator;
            ComponentsGenerator = componentsGenerator;
            TagGroupsGenerator = tagGroupsGenerator;
        }

        public OpenApiDocument GenerateDocument(Assembly assembly)
        {

            var generatorContext = new GeneratorContext(assembly);

            InfoGenerator.GenerateInfo(generatorContext);
            ComponentsGenerator.GenerateComponents(generatorContext);
            TagGroupsGenerator.GenerateTagGroups(generatorContext);
            PathsGenerator.GeneratePaths(generatorContext);

            return generatorContext.Document;
        }
    }

}
