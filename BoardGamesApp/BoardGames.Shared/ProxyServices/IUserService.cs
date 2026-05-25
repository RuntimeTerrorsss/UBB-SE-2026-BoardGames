using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IUserService
    {
        Task<ServiceResult<IReadOnlyList<UserDTO>>> GetUsersExceptAsync(Guid excludeAccountId, CancellationToken cancellationToken = default);
    }
}
