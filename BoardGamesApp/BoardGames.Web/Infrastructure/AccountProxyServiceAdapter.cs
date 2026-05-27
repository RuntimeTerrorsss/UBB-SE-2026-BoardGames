// <copyright file="AccountProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AccountProxyServiceAdapter : IAccountProxyService
    {
        private readonly IAccountService accountService;

        public AccountProxyServiceAdapter(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public async Task<AccountProfileDTO> GetProfileAsync(Guid accountId)
            => (await this.accountService.GetProfileAsync(accountId)).ThrowIfFailed();

        public async Task UpdateProfileAsync(Guid accountId, AccountProfileDTO updateData)
            => (await this.accountService.UpdateProfileAsync(accountId, updateData)).ThrowIfFailed();

        public async Task UploadAvatarAsync(Guid accountId, string imagePath)
            => (await this.accountService.UploadAvatarAsync(accountId, imagePath)).ThrowIfFailed();

        public async Task RemoveAvatarAsync(Guid accountId)
            => (await this.accountService.RemoveAvatarAsync(accountId)).ThrowIfFailed();

        public async Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
            => (await this.accountService.ChangePasswordAsync(accountId, currentPassword, newPassword)).ThrowIfFailed();
    }
}
