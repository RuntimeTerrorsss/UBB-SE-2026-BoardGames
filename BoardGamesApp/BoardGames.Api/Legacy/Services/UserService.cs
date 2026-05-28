using BoardGames.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Api.Legacy.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User?> LoginAsync(string identifier, string password)
        {

            var user = await _userRepository.Login(identifier, password);

            if (user == null)
            {
                return null;
            }

            if (user.IsSuspended)
            {

                return null;
            }

            return user;
        }

        private bool AreFieldsEmpty(User newUser)
        {
            return string.IsNullOrEmpty(newUser.Username)
                || string.IsNullOrEmpty(newUser.DisplayName)
                || string.IsNullOrEmpty(newUser.Email)
                || string.IsNullOrEmpty(newUser.PasswordHash)
                || string.IsNullOrEmpty(newUser.City)
                || string.IsNullOrEmpty(newUser.Country);
        }

        public async Task<bool> RegisterUserAsync(User newUser)
        {
            if (AreFieldsEmpty(newUser))
            {
                return false;
            }

            return await _userRepository.Register(newUser);
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            return await _userRepository.GetUserBalance(userId);
        }

        public async Task UpdateBalanceAsync(int userId, decimal amount)
        {
            await _userRepository.UpdateBalance(userId, amount);
        }
    }
}
