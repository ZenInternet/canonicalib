
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.OpenApiExtensions;

namespace Zen.CanonicaLib.UI.Services
{
    
    public class TagGroupsGenerator
    {
        public static void GenerateTagGroups(GeneratorContext generatorContext)
        {

            var tags = generatorContext.Library.TagGroups?
                .SelectMany(tg => tg.Tags)
                .ToHashSet();

            //TODO find all tags in the assembly that aren't in the tag groups and add them as well
            var assemblyTags = generatorContext.DiscoveryService.FindControllerTags(generatorContext.Assembly);

            foreach (var assemblyTag in assemblyTags)
            {
                tags.Add(assemblyTag);
            }

            generatorContext.Document.Tags = tags;

            var tagGroups = new List<OpenApiTagGroup>();

            if (assemblyTags.Any())
            {
                tagGroups.Add(new OpenApiTagGroup()
                {
                    Name = assemblyTags.Count == 1 ? "Capability" : "Capabilities",
                    Tags = assemblyTags.ToList()
                });
            }

            foreach (var tagGroup in generatorContext.Library.TagGroups)
            {
                tagGroups.Add(tagGroup);
            }


            if (tagGroups.Any())
            {
                generatorContext.Document.Extensions!.Add("x-tagGroups", new TagGroupsExtension(tagGroups));
            }

        }

    }
}