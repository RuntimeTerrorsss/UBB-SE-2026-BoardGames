using System.Threading.Tasks;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject dto);

        Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject dto);

        Task<ServiceResult<bool>> LogoutAsync();

        Task<ServiceResult<string>> ForgotPasswordAsync();
    }
}
