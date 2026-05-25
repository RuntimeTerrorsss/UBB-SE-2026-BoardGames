using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IFailedLoginRepository
    {
        Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId);
        Task IncrementAsync(Guid accountId);
        Task ResetAsync(Guid accountId);
    }
}
