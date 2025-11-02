using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.OpenApiExtensions;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface ITagGroupsGenerator
    {
        ISet<OpenApiTag>? GenerateTags(GeneratorContext generatorContext);

        TagGroupsExtension? GenerateTagGroups(GeneratorContext generatorContext);
    }
}