// <copyright file="ApiServiceBase.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.ProxyServices
{
    public abstract class ApiServiceBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        protected ApiServiceBase(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected HttpClient CreateClient() => this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
    }
}
