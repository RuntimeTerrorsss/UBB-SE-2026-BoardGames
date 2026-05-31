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

        public AdminProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync()
        {
            using var response = await this.httpClient.GetAsync($"admin/accounts?page={FirstPage}&pageSize={DefaultPageSize}");
            var accounts = await HttpProxyClient.ReadAsync<List<AccountProfileDTO>>(response);
            return accounts.Select(Map).ToList();
        }

        public async Task SuspendAccountAsync(string accountId)
        {
            using var response = await this.httpClient.PutAsync($"admin/accounts/{Guid.Parse(accountId)}/suspend", content: null);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task UnsuspendAccountAsync(string accountId)
        {
            using var response = await this.httpClient.PutAsync($"admin/accounts/{Guid.Parse(accountId)}/unsuspend", content: null);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task UnlockAccountAsync(string accountId)
        {
            using var response = await this.httpClient.PutAsync($"admin/accounts/{Guid.Parse(accountId)}/unlock", content: null);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task ResetPasswordAsync(string accountId, string newPassword)
        {
            var body = new ResetPasswordDTO { NewPassword = newPassword };
            using var response = await this.httpClient.PutAsJsonAsync($"admin/accounts/{Guid.Parse(accountId)}/reset-password", body);
            await HttpProxyClient.EnsureSuccessAsync(response);
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
