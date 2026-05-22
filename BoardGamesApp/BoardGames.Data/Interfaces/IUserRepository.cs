//// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
//// Copyright (c) PlaceholderCompany. All rights reserved.
//// </copyright>

namespace BoardGames.Data.Interfaces
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
