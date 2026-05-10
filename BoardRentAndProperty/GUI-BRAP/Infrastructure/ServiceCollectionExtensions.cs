using GUI_BRAP.ProxyServices;
using Microsoft.Extensions.DependencyInjection;

namespace GUI_BRAP.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxyServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthProxyService, AuthProxyService>();
            services.AddScoped<IGameProxyService, GameProxyService>();
            return services;
        }
    }
}
