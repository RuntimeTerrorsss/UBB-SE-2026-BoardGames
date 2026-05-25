using BoardGames.Data.Models;

namespace BoardGames.Api.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> LoginAsync(string identifier, string password);
        Task<bool> RegisterUserAsync(User newUser);
        Task<decimal> GetBalanceAsync(int userId);
        Task UpdateBalanceAsync(int userId, decimal amount);
    }
}
