using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using Xunit;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI;
using Zen.CanonicaLib.UI.Services;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Tests.Services
{
    // A test contract surface exercised by the security generator.
    [OpenApiTag("Widgets")]
    [OpenApiPath("/widgets")]
    public interface IWidgetsTestController
    {
        [OpenApiEndpoint("", "GET")]
        [OpenApiSecurity("oauth2", new[] { "read" })]
        void GetWidgets();

        // Two schemes on one operation — only valid once AllowMultiple = true.
        [OpenApiEndpoint("", "POST")]
        [OpenApiSecurity("oauth2", new[] { "write" })]
        [OpenApiSecurity("apiKey")]
        void CreateWidget();

        // References an undefined scope and an undefined scheme.
        [OpenApiEndpoint("bad", "GET")]
        [OpenApiSecurity("oauth2", new[] { "does-not-exist" })]
        [OpenApiSecurity("ghost-scheme", new[] { "x" })]
        void BadWidget();
    }

    public class DefaultSecurityGeneratorTests
    {
        private readonly Mock<IDiscoveryService> _discovery = new();
        private readonly Assembly _assembly = typeof(DefaultSecurityGeneratorTests).Assembly;

        private DefaultSecurityGenerator CreateSut() =>
            new(_discovery.Object, Mock.Of<ILogger<DefaultSecurityGenerator>>());

        private static MethodInfo Method(string name) =>
            typeof(IWidgetsTestController).GetMethod(name)!;

        // A generator context whose document already declares the schemes, so security-scheme
        // references built against it resolve (and therefore serialize by name).
        private GeneratorContext ContextWithSchemes()
        {
            var context = new GeneratorContext(_assembly);
            context.Document.Components!.SecuritySchemes = TwoSchemes();
            return context;
        }

        private static IDictionary<string, IOpenApiSecurityScheme> TwoSchemes() =>
            new Dictionary<string, IOpenApiSecurityScheme>
            {
                {
                    "oauth2",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://example.com/authorize"),
                                TokenUrl = new Uri("https://example.com/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "read", "Read widgets" },
                                    { "write", "Write widgets" }
                                }
                            }
                        }
                    }
                },
                {
                    "apiKey",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        Name = "X-Api-Key",
                        In = ParameterLocation.Header
                    }
                }
            };

        [Fact]
        public void GenerateOperationSecurityRequirements_EmitsOneRequirementPerAttribute()
        {
            var requirements = CreateSut().GenerateOperationSecurityRequirements(
                Method(nameof(IWidgetsTestController.CreateWidget)), ContextWithSchemes());

            // Two [OpenApiSecurity] attributes → two requirements (only possible with AllowMultiple = true).
            requirements.Should().HaveCount(2);
        }

        [Fact]
        public void GenerateOperationSecurityRequirements_CarriesRequestedScopes()
        {
            var requirements = CreateSut().GenerateOperationSecurityRequirements(
                Method(nameof(IWidgetsTestController.GetWidgets)), ContextWithSchemes());

            requirements.Should().ContainSingle();
            requirements[0].Values.Single().Should().Contain("read");
        }

        [Fact]
        public void GenerateOperationSecurityRequirements_SerializesSchemeReference_NotEmptyObject()
        {
            // Regression guard: the scheme reference must carry the host document, otherwise the
            // requirement serializes as "{}" and neither the scheme nor its scopes reach the docs.
            var requirements = CreateSut().GenerateOperationSecurityRequirements(
                Method(nameof(IWidgetsTestController.GetWidgets)), ContextWithSchemes());

            using var sw = new StringWriter();
            requirements.Single().SerializeAsV31(new OpenApiJsonWriter(sw));
            var json = sw.ToString();

            json.Should().Contain("oauth2");
            json.Should().Contain("read");
            json.Should().NotBe("{ }");
        }

        [Fact]
        public void GenerateSecuritySchemes_ReturnsSchemes_FromSecureService()
        {
            var secure = new Mock<ISecureService>();
            secure.Setup(s => s.SecuritySchemes).Returns(TwoSchemes());
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns(secure.Object);

            var schemes = CreateSut().GenerateSecuritySchemes(new GeneratorContext(_assembly));

            schemes.Keys.Should().BeEquivalentTo(new[] { "oauth2", "apiKey" });
        }

        [Fact]
        public void GenerateSecuritySchemes_ReturnsEmpty_WhenNoSecureService()
        {
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns((ISecureService?)null);

            var schemes = CreateSut().GenerateSecuritySchemes(new GeneratorContext(_assembly));

            schemes.Should().BeEmpty();
        }

        [Fact]
        public void GenerateDocumentSecurityRequirements_ReturnsRootSecurity_FromSecureService()
        {
            var rootSecurity = new List<OpenApiSecurityRequirement>
            {
                new() { { new OpenApiSecuritySchemeReference("oauth2"), new List<string>() } }
            };
            var secure = new Mock<ISecureService>();
            secure.Setup(s => s.Security).Returns(rootSecurity);
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns(secure.Object);

            var result = CreateSut().GenerateDocumentSecurityRequirements(new GeneratorContext(_assembly));

            result.Should().HaveCount(1);
        }

        [Fact]
        public void GenerateDocumentSecurityRequirements_ReturnsEmpty_WhenSecurityNull()
        {
            var secure = new Mock<ISecureService>();
            secure.Setup(s => s.Security).Returns((IList<OpenApiSecurityRequirement>?)null);
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns(secure.Object);

            var result = CreateSut().GenerateDocumentSecurityRequirements(new GeneratorContext(_assembly));

            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateSecurity_FlagsUndefinedSchemeAndUndefinedScope()
        {
            var secure = new Mock<ISecureService>();
            secure.Setup(s => s.SecuritySchemes).Returns(TwoSchemes());
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns(secure.Object);
            _discovery.Setup(d => d.FindControllerDefinitions(It.IsAny<Assembly>()))
                .Returns(new List<Type> { typeof(IWidgetsTestController) });
            _discovery.Setup(d => d.FindEndpointDefinitions(typeof(IWidgetsTestController)))
                .Returns(new List<MethodInfo>
                {
                    Method(nameof(IWidgetsTestController.GetWidgets)),
                    Method(nameof(IWidgetsTestController.CreateWidget)),
                    Method(nameof(IWidgetsTestController.BadWidget))
                });

            var warnings = CreateSut().ValidateSecurity(new GeneratorContext(_assembly));

            // Exactly the two problems on BadWidget; the valid operations produce nothing.
            warnings.Should().HaveCount(2);
            warnings.Should().Contain(w => w.Contains("ghost-scheme") && w.Contains("not declared"));
            warnings.Should().Contain(w => w.Contains("does-not-exist") && w.Contains("not defined"));
        }

        [Fact]
        public void ValidateSecurity_ReturnsNoWarnings_WhenConfigurationIsConsistent()
        {
            var secure = new Mock<ISecureService>();
            secure.Setup(s => s.SecuritySchemes).Returns(TwoSchemes());
            _discovery.Setup(d => d.GetSecureServiceInstance(It.IsAny<Assembly>())).Returns(secure.Object);
            _discovery.Setup(d => d.FindControllerDefinitions(It.IsAny<Assembly>()))
                .Returns(new List<Type> { typeof(IWidgetsTestController) });
            _discovery.Setup(d => d.FindEndpointDefinitions(typeof(IWidgetsTestController)))
                .Returns(new List<MethodInfo>
                {
                    Method(nameof(IWidgetsTestController.GetWidgets)),
                    Method(nameof(IWidgetsTestController.CreateWidget))
                });

            var warnings = CreateSut().ValidateSecurity(new GeneratorContext(_assembly));

            warnings.Should().BeEmpty();
        }
    }
}
