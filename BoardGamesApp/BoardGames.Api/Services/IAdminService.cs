using BoardGames.Shared.DTO;
using BoardGames.Shared.Common;

namespace BoardGames.Api.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<List<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int page, int pageSize);
        Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId);
        Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId);
        Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword);
        Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId);
    }
}
