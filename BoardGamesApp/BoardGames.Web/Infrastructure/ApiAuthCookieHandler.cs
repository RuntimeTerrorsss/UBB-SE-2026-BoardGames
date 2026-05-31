// <copyright file="ApiAuthCookieHandler.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    /// <summary>
    /// Forwards stored API auth cookies on outbound requests and captures new ones from responses.
    /// </summary>
    public sealed class ApiAuthCookieHandler : DelegatingHandler
    {
        private readonly IApiAuthCookieStore cookieStore;

        public ApiAuthCookieHandler(IApiAuthCookieStore cookieStore)
        {
            this.cookieStore = cookieStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            this.cookieStore.ApplyToRequest(request);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (request.RequestUri is not null)
            {
                this.cookieStore.SaveFromResponse(response, request.RequestUri);
            }

            return response;
        }
    }
}
