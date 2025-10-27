using CanonicaLib.UI.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CanonicaLib.UI
{
    public static class WebApplicationExtensions
    {
        public static IEndpointRouteBuilder UseCanonicaLib(this IEndpointRouteBuilder app, Func<WebApplicationOptions> optionsFactory)
        {
            var options = optionsFactory.Invoke();

            app.MapGet($"{options.RootPath}", RootEndpointHandler.HandleRootRequest);

            app.MapGet($"{options.RootPath}/{{**slug}}", AssemblyEndpointHandler.HandleAssemblyRequest);

            return app;
        }

        public static IEndpointRouteBuilder UseCanonicaLib(this IEndpointRouteBuilder app)
        {
            return app.UseCanonicaLib(() => new WebApplicationOptions());
        }
    }
}