// <copyright file="ServiceCollectionExtensions.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
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
