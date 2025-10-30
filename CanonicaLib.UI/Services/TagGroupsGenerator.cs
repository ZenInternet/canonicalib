
using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.OpenApiExtensions;

namespace Zen.CanonicaLib.UI.Services
{
    public class TagGroupsGenerator
    {
        private readonly DiscoveryService DiscoveryService;

        public TagGroupsGenerator(DiscoveryService discoveryService)
        {
            DiscoveryService = discoveryService;
        }

        public void GenerateTagGroups(GeneratorContext generatorContext)
        {
            var library = DiscoveryService.GetLibraryInstance(generatorContext.Assembly);
            var tags = library.TagGroups?
                .SelectMany(tg => tg.Tags)
                .ToHashSet() ?? new HashSet<OpenApiTag>();

            //TODO find all tags in the assembly that aren't in the tag groups and add them as well
            var assemblyTags = DiscoveryService.FindControllerTags(generatorContext.Assembly);

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

            if (library.TagGroups != null)
            {
                foreach (var tagGroup in library.TagGroups)
                {
                    tagGroups.Add(tagGroup);
                }
            }

            if (tagGroups.Any())
            {
                generatorContext.Document.Extensions!.Add("x-tagGroups", new TagGroupsExtension(tagGroups));
            }
        }
    }
}