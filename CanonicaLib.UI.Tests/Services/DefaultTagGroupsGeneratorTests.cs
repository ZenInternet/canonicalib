using System.Reflection;
using FluentAssertions;
using Microsoft.OpenApi;
using Moq;
using Xunit;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI;
using Zen.CanonicaLib.UI.Services;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Tests.Services
{
    public class DefaultTagGroupsGeneratorTests
    {
        private readonly Mock<IDiscoveryService> _discovery = new();
        private readonly Assembly _assembly = typeof(DefaultTagGroupsGenerator).Assembly;

        private ISet<OpenApiTag>? Generate(string documentName, string documentContent)
        {
            var library = new Mock<ILibrary>();
            library.Setup(l => l.TagGroups).Returns((IList<OpenApiTagGroup>?)null);

            _discovery.Setup(d => d.GetLibraryInstance(It.IsAny<Assembly>())).Returns(library.Object);
            _discovery.Setup(d => d.FindControllerTags(It.IsAny<Assembly>())).Returns(new HashSet<OpenApiTag>());
            _discovery.Setup(d => d.FindWebhookTags(It.IsAny<Assembly>())).Returns(new HashSet<OpenApiTag>());
            _discovery.Setup(d => d.GetDocumentList(It.IsAny<Assembly>())).Returns(new List<string> { documentName });
            _discovery.Setup(d => d.GetDocumentContent(It.IsAny<Assembly>(), documentName)).Returns(documentContent);

            var generator = new DefaultTagGroupsGenerator(_discovery.Object);
            return generator.GenerateTags(new GeneratorContext(_assembly));
        }

        [Fact]
        public void GenerateTags_PreservesBlankLines_AndStripsH1Title()
        {
            // Arrange — a doc whose structure depends on blank lines (paragraphs, a heading, a list,
            // and a fenced code block).
            var content =
                "# My Doc Title\n" +
                "\n" +
                "First paragraph.\n" +
                "\n" +
                "## A heading\n" +
                "\n" +
                "- item one\n" +
                "- item two\n" +
                "\n" +
                "```json\n" +
                "{ \"a\": 1 }\n" +
                "```\n";

            // Act
            var tags = Generate("Example.md", content);

            // Assert
            var tag = tags!.Single(t => t.Name == "My Doc Title");
            var description = tag.Description!;

            // The H1 line is consumed as the tag name, not left in the body.
            description.Should().NotContain("# My Doc Title");
            description.Should().StartWith("First paragraph.");

            // Blank lines between blocks are preserved — this is what makes markdown render correctly.
            description.Should().Contain("\n\n", "blank lines separate markdown blocks and must survive");
            description.Should().Contain("First paragraph.\n\n## A heading");
            description.Should().Contain("## A heading\n\n- item one");
            description.Should().Contain("- item two\n\n```json");
        }

        [Fact]
        public void GenerateTags_UsesFriendlyFileName_WhenNoH1()
        {
            var content = "Just a paragraph.\n\nAnother paragraph.\n";

            var tags = Generate("SupplierLineMappingRules.md", content);

            var tag = tags!.Single(t => t.Description != null && t.Description.StartsWith("Just a paragraph."));
            tag.Name.Should().Be("Supplier Line Mapping Rules");
            tag.Description.Should().Contain("\n\n");
        }
    }
}
