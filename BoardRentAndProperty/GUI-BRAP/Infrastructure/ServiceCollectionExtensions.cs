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
            services.AddScoped<INotificationProxyService, NotificationProxyService>();
            services.AddScoped<IAdminProxyService, AdminProxyService>();
            services.AddScoped<IRequestProxyService, RequestProxyService>();
            return services;
        }
    }
}
