// <copyright file="AuthProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using System.Net;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AuthProxyServiceAdapter : IAuthProxyService
    {
        private readonly HttpClient httpClient;

        public AuthProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<AccountProfileDTO> LoginAsync(LoginDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("auth/login", body, cancellationToken);
            return await HttpProxyClient.ReadAsync<AccountProfileDTO>(response, cancellationToken);
        }

        public async Task RegisterAsync(RegisterDTO body, CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsJsonAsync("auth/register", body, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.PostAsync("auth/logout", content: null, cancellationToken);
            await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default)
        {
            using var response = await this.httpClient.GetAsync("auth/forgot-password", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateNotConnectedAwareExceptionAsync(response, cancellationToken);
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private static async Task<ProxyServiceException> CreateNotConnectedAwareExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                await HttpProxyClient.EnsureSuccessAsync(response, cancellationToken);
            }
            catch (ProxyServiceException ex)
            {
                return ex;
            }

            return new ProxyServiceException("Unexpected API error.", HttpStatusCode.InternalServerError, null);
        }
    }
}
