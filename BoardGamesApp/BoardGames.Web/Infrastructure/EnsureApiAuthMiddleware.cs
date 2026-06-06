// <copyright file="EnsureApiAuthMiddleware.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BoardGames.Web.Infrastructure
{
    /// <summary>
    /// Signs the user out when the MVC auth cookie is present but API auth cookies are missing
    /// (e.g. after a restart before API cookies were persisted).
    /// </summary>
    public sealed class EnsureApiAuthMiddleware
    {
        private readonly RequestDelegate next;

        public EnsureApiAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApiAuthCookieStore apiAuthCookieStore)
        {
            if (ShouldValidate(context)
                && context.User.Identity?.IsAuthenticated == true
                && !apiAuthCookieStore.HasStoredCookies())
            {
                apiAuthCookieStore.Clear();
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Auth/Login?apiSession=true");
                return;
            }

            await this.next(context);
        }

        private static bool ShouldValidate(HttpContext context)
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                return false;
            }

            string path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
