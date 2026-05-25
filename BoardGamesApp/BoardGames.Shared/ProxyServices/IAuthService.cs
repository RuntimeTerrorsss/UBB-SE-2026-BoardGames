using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDataTransferObject request, CancellationToken cancellationToken = default);

        Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject request, CancellationToken cancellationToken = default);

        Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default);
    }
}
