using CanonicaLib.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CanonicaLib.UI
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddCanonicaLib(this IServiceCollection services)
        {
            services.AddTransient<DiscoveryService>();
            
            services.AddTransient<DocumentGenerator>();

            services.AddTransient<InfoGenerator>();
            
            services.AddTransient<PathsGenerator>();
            services.AddTransient<OperationGenerator>();
            services.AddTransient<ResponsesGenerator>();

            services.AddTransient<ComponentsGenerator>();
            services.AddTransient<SchemasGenerator>();
            services.AddTransient<SchemaGenerator>();

            return services;
        }
    }
}