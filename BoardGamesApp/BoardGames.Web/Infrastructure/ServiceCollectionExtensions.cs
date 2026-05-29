// <copyright file="ServiceCollectionExtensions.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxyServices(this IServiceCollection services, Uri apiBaseAddress)
        {
            services.AddHttpClient<IAuthProxyService, AuthProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IGameProxyService, GameProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IRentalProxyService, RentalProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IRequestProxyService, RequestProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<INotificationProxyService, NotificationProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IAccountProxyService, AccountProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IAdminProxyService, AdminProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IChatProxyService, ConversationProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IConversationProxyService, ConversationProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            services.AddHttpClient<IPaymentProxyService, PaymentProxyServiceAdapter>(client => client.BaseAddress = apiBaseAddress);
            return services;
        }
    }
}
