// <copyright file="ServiceCollectionExtensions.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxyServices(this IServiceCollection services, Uri apiBaseAddress)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IApiAuthCookieStore, SessionApiAuthCookieStore>();
            services.AddTransient<ApiAuthCookieHandler>();

            RegisterApiClient<IAuthProxyService, AuthProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IGameProxyService, GameProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IRentalProxyService, RentalProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IRequestProxyService, RequestProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<INotificationProxyService, NotificationProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IAccountProxyService, AccountProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IAdminProxyService, AdminProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IChatProxyService, ConversationProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IConversationProxyService, ConversationProxyServiceAdapter>(services, apiBaseAddress);
            RegisterApiClient<IPaymentProxyService, PaymentProxyServiceAdapter>(services, apiBaseAddress);

            return services;
        }

        private static void RegisterApiClient<TClient, TImplementation>(
            IServiceCollection services,
            Uri apiBaseAddress)
            where TClient : class
            where TImplementation : class, TClient
        {
            services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = apiBaseAddress)
                .AddHttpMessageHandler<ApiAuthCookieHandler>();
        }
    }
}
