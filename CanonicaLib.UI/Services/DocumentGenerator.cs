
using Microsoft.OpenApi;
using System.Reflection;

namespace CanonicaLib.UI.Services
{
    public class DocumentGenerator
    {
        private readonly InfoGenerator InfoGenerator;
        private readonly PathsGenerator PathsGenerator;
        private readonly ComponentsGenerator ComponentsGenerator;

        public DocumentGenerator(InfoGenerator infoGenerator, PathsGenerator pathsGenerator, ComponentsGenerator componentsGenerator)
        {
            InfoGenerator = infoGenerator;
            PathsGenerator = pathsGenerator;
            ComponentsGenerator = componentsGenerator;
        }

        public OpenApiDocument GenerateDocument(Assembly assembly)
        {
            var document = new OpenApiDocument
            {
                Info = InfoGenerator.GenerateInfo(assembly),
                Paths = PathsGenerator.GeneratePaths(assembly),
                Components = ComponentsGenerator.GenerateComponents(assembly),
            };

            return document;
        }
    }

}
