
using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.OpenApiExtensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
    public class DefaultTagGroupsGenerator : ITagGroupsGenerator
    {
        private readonly IDiscoveryService DiscoveryService;

        public DefaultTagGroupsGenerator(IDiscoveryService discoveryService)
        {
            DiscoveryService = discoveryService;
        }

        public ISet<OpenApiTag>? GenerateTags(GeneratorContext generatorContext)
        {
            var library = DiscoveryService.GetLibraryInstance(generatorContext.Assembly);
            var tags = library.TagGroups?
                .SelectMany(tg => tg.Tags)
                .ToHashSet() ?? new HashSet<OpenApiTag>();

            //TODO find all tags in the assembly that aren't in the tag groups and add them as well
            var controllerTags = DiscoveryService.FindControllerTags(generatorContext.Assembly);
            var webhookTags = DiscoveryService.FindWebhookTags(generatorContext.Assembly);

            var documents = DiscoveryService.GetDocumentList(generatorContext.Assembly).Where(x => !x.EndsWith("Index.md", StringComparison.InvariantCultureIgnoreCase));

            foreach (var document in documents)
            {
                var description = DiscoveryService.GetDocumentContent(generatorContext.Assembly, document);
                // if the first line of the description is a markdown #1, use that as the name, remove it from the description
                var lines = description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0 && lines[0].StartsWith("# "))
                {
                    var title = lines[0].Substring(2).Trim();
                    description = string.Join(Environment.NewLine, lines.Skip(1)).Trim();

                    tags.Add(new OpenApiTag()
                    {
                        Name = title,
                        Description = description.IfEmpty(null)
                    });
                }
                else
                {
                    tags.Add(new OpenApiTag()
                    {
                        Name = document.Replace(".md", "").ToFriendlyName(),
                        Description = description.IfEmpty(null)
                    });
                }
            }

            foreach (var controllerTag in controllerTags)
            {
                tags.Add(controllerTag);
            }

            foreach (var webhookTag in webhookTags)
            {
                tags.Add(webhookTag);
            }

            return tags;
        }

        public TagGroupsExtension? GenerateTagGroups(GeneratorContext generatorContext)
        {
            var library = DiscoveryService.GetLibraryInstance(generatorContext.Assembly);
            var tags = library.TagGroups?
                .SelectMany(tg => tg.Tags)
                .ToHashSet() ?? new HashSet<OpenApiTag>();

            //TODO find all tags in the assembly that aren't in the tag groups and add them as well
            var controllerTags = DiscoveryService.FindControllerTags(generatorContext.Assembly);
            var webhookTags = DiscoveryService.FindWebhookTags(generatorContext.Assembly);

            var documents = DiscoveryService.GetDocumentList(generatorContext.Assembly).Where(x => !x.EndsWith("Index.md", StringComparison.InvariantCultureIgnoreCase));

            var tagGroups = new List<OpenApiTagGroup>();

            if (documents.Any())
            {
                tagGroups.Add(new OpenApiTagGroup()
                {
                    Name = "Documentation",
                    Tags = documents.Select(x => new OpenApiTag()
                    {
                        Name = x.Replace(".md", "").ToFriendlyName()
                    }).ToList(),
                });
            }

            if (controllerTags.Any())
            {
                tagGroups.Add(new OpenApiTagGroup()
                {
                    Name = controllerTags.Count == 1 ? "Capability" : "Capabilities",
                    Tags = controllerTags.ToList()
                });
            }

            if (webhookTags.Any())
            {
                tagGroups.Add(new OpenApiTagGroup()
                {
                    Name = webhookTags.Count == 1 ? "Webhook" : "Webhooks",
                    Tags = webhookTags.ToList()
                });
            }

            if (library.TagGroups != null)
            {
                foreach (var tagGroup in library.TagGroups)
                {
                    tagGroups.Add(tagGroup);
                }
            }
            
            return tagGroups.Any() ? new TagGroupsExtension(tagGroups) : null;
        }
    }
}