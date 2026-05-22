using System;
using System.Threading.Tasks;
using BoardGames.Web.Infrastructure;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AccountProxyServiceAdapter : IAccountProxyService
    {
        private readonly IAccountService accountService;

        public AccountProxyServiceAdapter(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public async Task<AccountProfileDataTransferObject> GetProfileAsync(Guid accountId)
            => (await accountService.GetProfileAsync(accountId)).ThrowIfFailed();

        public async Task UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject updateData)
            => (await accountService.UpdateProfileAsync(accountId, updateData)).ThrowIfFailed();

        public async Task UploadAvatarAsync(Guid accountId, string imagePath)
            => (await accountService.UploadAvatarAsync(accountId, imagePath)).ThrowIfFailed();

        public async Task RemoveAvatarAsync(Guid accountId)
            => (await accountService.RemoveAvatarAsync(accountId)).ThrowIfFailed();

        public async Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
            => (await accountService.ChangePasswordAsync(accountId, currentPassword, newPassword)).ThrowIfFailed();
    }
}
