// <copyright file="AdminProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Models.Account;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AdminProxyServiceAdapter : IAdminProxyService
    {
        private const int FirstPage = 1;
        private const int DefaultPageSize = 100;

        private readonly HttpClient httpClient;
        private readonly IApiAuthCookieStore apiAuthCookieStore;

        public AdminProxyServiceAdapter(HttpClient httpClient, IApiAuthCookieStore apiAuthCookieStore)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.apiAuthCookieStore = apiAuthCookieStore ?? throw new ArgumentNullException(nameof(apiAuthCookieStore));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync()
        {
            using var request = CreateRequest(HttpMethod.Get, $"admin/accounts?page={FirstPage}&pageSize={DefaultPageSize}");
            using var response = await this.httpClient.SendAsync(request);
            var accounts = await HttpProxyClient.ReadAsync<List<AccountProfileDTO>>(response);
            return accounts.Select(Map).ToList();
        }

        public async Task SuspendAccountAsync(string accountId)
        {
            using var request = CreateRequest(HttpMethod.Put, $"admin/accounts/{Guid.Parse(accountId)}/suspend");
            using var response = await this.httpClient.SendAsync(request);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task UnsuspendAccountAsync(string accountId)
        {
            using var request = CreateRequest(HttpMethod.Put, $"admin/accounts/{Guid.Parse(accountId)}/unsuspend");
            using var response = await this.httpClient.SendAsync(request);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task UnlockAccountAsync(string accountId)
        {
            using var request = CreateRequest(HttpMethod.Put, $"admin/accounts/{Guid.Parse(accountId)}/unlock");
            using var response = await this.httpClient.SendAsync(request);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task ResetPasswordAsync(string accountId, string newPassword)
        {
            var body = new ResetPasswordDTO { NewPassword = newPassword };
            using var request = CreateRequest(
                HttpMethod.Put,
                $"admin/accounts/{Guid.Parse(accountId)}/reset-password",
                JsonContent.Create(body));
            using var response = await this.httpClient.SendAsync(request);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, relativeUrl)
            {
                Content = content,
            };

            this.apiAuthCookieStore.ApplyTo(request);
            return request;
        }

        private static AdminAccountViewModel Map(AccountProfileDTO profile) => new()
        {
            Id = profile.Id.ToString(),
            Username = profile.Username,
            Email = profile.Email,
            Role = new RoleViewModel
            {
                Id = profile.Role?.Id.ToString() ?? string.Empty,
                Name = profile.Role?.Name ?? string.Empty,
            },
            IsSuspended = profile.IsSuspended,
            IsLockedOut = profile.IsLocked,
        };
    }
}
