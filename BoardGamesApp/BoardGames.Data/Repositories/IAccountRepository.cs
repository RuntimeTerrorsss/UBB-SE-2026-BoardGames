// <copyright file="IAccountRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IAccountRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByUsernameAsync(string username);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByPamUserIdAsync(int pamUserId);

        Task<List<User>> GetAllAsync(int page, int pageSize);

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task AddRoleAsync(Guid accountId, string roleName);
    }
}
