using System;
using Microsoft.Extensions.DependencyInjection;

namespace BoardRentAndProperty.ApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBoardRentApiClient(this IServiceCollection services, Action<ApiClientOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new ApiClientOptions();
            configure(options);

            if (options.BaseAddress is null)
            {
                throw new InvalidOperationException("ApiClientOptions.BaseAddress must be set before calling AddBoardRentApiClient.");
            }

            services.AddHttpClient(ApiClientNames.BoardRentApi, client =>
            {
                client.BaseAddress = options.BaseAddress;
                if (options.Timeout is { } timeout)
                {
                    client.Timeout = timeout;
                }
            });

            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IGameService, GameService>();
            services.AddTransient<IRentalService, RentalService>();
            services.AddTransient<IRequestService, RequestService>();
            services.AddTransient<INotificationService, NotificationService>();

            return services;
        }
    }
}
