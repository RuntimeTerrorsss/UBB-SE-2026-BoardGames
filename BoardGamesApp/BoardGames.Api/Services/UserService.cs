// <copyright file="UserService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Data.Repositories;

namespace BoardGames.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepositoryParam)
        {
            this.userRepository = userRepositoryParam;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await this.userRepository.GetById(id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await this.userRepository.GetAll();
        }

        public async Task<User?> LoginAsync(string identifier, string password)
        {
            var user = await this.userRepository.Login(identifier, password);

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
            if (this.AreFieldsEmpty(newUser))
            {
                return false;
            }

            return await this.userRepository.Register(newUser);
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            return await this.userRepository.GetUserBalance(userId);
        }

        public async Task UpdateBalanceAsync(int userId, decimal amount)
        {
            await this.userRepository.UpdateBalance(userId, amount);
        }
    }
}
