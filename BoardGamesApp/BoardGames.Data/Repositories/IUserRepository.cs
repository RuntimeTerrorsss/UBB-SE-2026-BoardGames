// <copyright file="IUserRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User?> GetById(int id);

        public Task SaveAddress(int id, Address address);

        public Task<decimal> GetUserBalance(int userId);

        public Task UpdateBalance(int userId, decimal newBalance);

        Task<User?> Login(string emailOrUsername, string password);

        Task<bool> Register(User newUser);
    }
}
