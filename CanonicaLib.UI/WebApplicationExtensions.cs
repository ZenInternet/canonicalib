using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zen.CanonicaLib.UI.Handlers;

namespace Zen.CanonicaLib.UI
{
    public static class WebApplicationExtensions
    {
        public static IEndpointRouteBuilder UseCanonicaLib(this IEndpointRouteBuilder app)
        {
            var options = app.ServiceProvider.GetRequiredService<WebApplicationOptions>() ?? new WebApplicationOptions();

            app.MapGet($"{options.RootPath}", UIEndpointHandler.HandleUIRequest);

            app.MapGet($"{options.RootPath}/{{**slug}}", RedoclyEndpointHandler.HandleRedoclyRequest);

            app.MapGet($"{options.RootPath}{options.ApiPath}/", AssembliesEndpointHandler.HandleAssembliesRequest);

            app.MapGet($"{options.RootPath}{options.ApiPath}/{{**slug}}", AssemblyEndpointHandler.HandleAssemblyRequest);

            return app;
        }
    }
}