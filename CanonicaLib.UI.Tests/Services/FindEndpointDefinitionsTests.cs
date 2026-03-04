using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.Services;

namespace CanonicaLib.UI.Tests.Services;

// Simulates the CPE webhook pattern: base has [OpenApiEndpoint], child uses 'new'
[OpenApiTag("Events")]
public interface IWebhookBase
{
    [OpenApiEndpoint("", "POST")]
    void PostEvent(string payload);
}

[OpenApiWebhook("testEvent")]
public interface IChildWebhook : IWebhookBase
{
    new void PostEvent(string payload);
}

// Simulates a webhook with no base type (works fine)
[OpenApiWebhook("simpleEvent")]
public interface ISimpleWebhook
{
    [OpenApiEndpoint("", "POST")]
    void PostEvent(string payload);
}

// Simulates a webhook where child does NOT redeclare with 'new'
[OpenApiWebhook("inheritedEvent")]
public interface IInheritedWebhook : IWebhookBase { }

public class FindEndpointDefinitionsTests
{
    private readonly DefaultDiscoveryService _sut = new();

    [Fact]
    public void SimpleWebhook_FindsEndpoint()
    {
        var endpoints = _sut.FindEndpointDefinitions(typeof(ISimpleWebhook));
        Assert.Single(endpoints);
        Assert.Equal("PostEvent", endpoints[0].Name);
    }

    [Fact]
    public void ChildWebhook_WithNew_FindsEndpoint()
    {
        var endpoints = _sut.FindEndpointDefinitions(typeof(IChildWebhook));
        Assert.Single(endpoints);
        Assert.Equal("PostEvent", endpoints[0].Name);
    }

    [Fact]
    public void ChildWebhook_WithNew_ReturnsChildMethodInfo()
    {
        var endpoints = _sut.FindEndpointDefinitions(typeof(IChildWebhook));
        Assert.Single(endpoints);
        // Should return the child's MethodInfo, not the parent's
        Assert.Equal(typeof(IChildWebhook), endpoints[0].DeclaringType);
    }

    [Fact]
    public void ChildWebhook_WithNew_GetEndpointAttribute_FindsParentAttribute()
    {
        var endpoints = _sut.FindEndpointDefinitions(typeof(IChildWebhook));
        Assert.Single(endpoints);

        // The child method itself won't have [OpenApiEndpoint]
        var directAttr = endpoints[0].GetCustomAttribute<OpenApiEndpointAttribute>();
        Assert.Null(directAttr);

        // But GetEndpointAttribute() should find it via the parent
        var attr = endpoints[0].GetEndpointAttribute();
        Assert.NotNull(attr);
        Assert.Equal("POST", attr.HttpMethod);
    }

    [Fact]
    public void InheritedWebhook_NoNew_FindsParentEndpoint()
    {
        var endpoints = _sut.FindEndpointDefinitions(typeof(IInheritedWebhook));
        Assert.Single(endpoints);
        Assert.Equal("PostEvent", endpoints[0].Name);
    }
}
