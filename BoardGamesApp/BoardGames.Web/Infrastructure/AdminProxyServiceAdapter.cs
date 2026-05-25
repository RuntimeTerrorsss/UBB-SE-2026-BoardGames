// <copyright file="AdminProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Web.Models.Account;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AdminProxyServiceAdapter : IAdminProxyService
    {
        private const int FirstPage = 1;
        private const int DefaultPageSize = 100;

        private readonly IAdminService adminService;

        public AdminProxyServiceAdapter(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        public async Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync()
        {
            var accounts = (await this.adminService.GetAllAccountsAsync(FirstPage, DefaultPageSize)).ThrowIfFailed();
            return accounts.Select(Map).ToList();
        }

        public async Task SuspendAccountAsync(string accountId)
            => (await this.adminService.SuspendAccountAsync(Guid.Parse(accountId))).ThrowIfFailed();

        public async Task UnsuspendAccountAsync(string accountId)
            => (await this.adminService.UnsuspendAccountAsync(Guid.Parse(accountId))).ThrowIfFailed();

        public async Task UnlockAccountAsync(string accountId)
            => (await this.adminService.UnlockAccountAsync(Guid.Parse(accountId))).ThrowIfFailed();

        public async Task ResetPasswordAsync(string accountId, string newPassword)
            => (await this.adminService.ResetPasswordAsync(Guid.Parse(accountId), newPassword)).ThrowIfFailed();

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
