using System;
using System.Threading.Tasks;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IAccountService
    {
        Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId);
        Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData);
        Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);
        Task<ServiceResult<string>> SetAvatarUrlAsync(Guid accountId, string avatarRelativeUrl);
        Task<ServiceResult<bool>> RemoveAvatarAsync(Guid accountId);
    }
}
