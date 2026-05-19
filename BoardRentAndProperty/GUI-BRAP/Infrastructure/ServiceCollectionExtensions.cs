using GUI_BRAP.ProxyServices;
using Microsoft.Extensions.DependencyInjection;

namespace GUI_BRAP.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxyServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthProxyService, AuthProxyServiceAdapter>();
            services.AddScoped<IAccountProxyService, AccountProxyServiceAdapter>();
            services.AddScoped<IAdminProxyService, AdminProxyServiceAdapter>();
            services.AddScoped<IGameProxyService, GameProxyServiceAdapter>();
            services.AddScoped<IRentalProxyService, RentalProxyServiceAdapter>();
            services.AddScoped<IRequestProxyService, RequestProxyServiceAdapter>();
            services.AddScoped<INotificationProxyService, NotificationProxyServiceAdapter>();
            return services;
        }
    }
}
