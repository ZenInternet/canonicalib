using CanonicaLib.UI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CanonicaLib.UI.Handlers
{
    public static class RootEndpointHandler
    {
        public static async Task HandleRootRequest(HttpContext context)
        {
            var discoveryService = context.RequestServices.GetRequiredService<DiscoveryService>();

            var assemblies = discoveryService.FindCanonicalAssemblies();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;

            var result = JsonSerializer.Serialize(new
            {
                Assemblies = assemblies.Select(a => a.FullName).ToList()
            });

            await context.Response.WriteAsync(result);
        }
    }
}