// <copyright file="AccountRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public AccountRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Users.Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User?> GetByPamUserIdAsync(int pamUserId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Users.FirstOrDefaultAsync(user => user.PamUserId == pamUserId);
        }

        public async Task<List<User>> GetAllAsync(int page, int pageSize)
        {
            const int pageOffset = 1;

            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Users
                .Include(user => user.Roles)
                .OrderBy(user => user.CreatedAt)
                .Skip((page - pageOffset) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = await dbContext.Users.FindAsync(user.Id);
            if (existing == null)
            {
                return;
            }

            existing.DisplayName = user.DisplayName;
            existing.Username = user.Username;
            existing.Email = user.Email;
            existing.PasswordHash = user.PasswordHash;
            existing.PhoneNumber = user.PhoneNumber ?? string.Empty;
            existing.AvatarUrl = user.AvatarUrl ?? string.Empty;
            existing.IsSuspended = user.IsSuspended;
            existing.CreatedAt = user.CreatedAt;
            existing.UpdatedAt = user.UpdatedAt;
            existing.Country = user.Country ?? string.Empty;
            existing.City = user.City ?? string.Empty;
            existing.StreetName = user.StreetName ?? string.Empty;
            existing.StreetNumber = user.StreetNumber ?? string.Empty;

            await dbContext.SaveChangesAsync();
        }

        public async Task AddRoleAsync(Guid accountId, string roleName)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            var role = await dbContext.Roles.FirstOrDefaultAsync(repositoryRole => repositoryRole.Name == roleName);
            if (role == null)
            {
                return;
            }

            bool alreadyHasRole = await dbContext.Set<AccountRole>()
                .AnyAsync(accountRole => accountRole.AccountId == accountId && accountRole.RoleId == role.Id);

            if (!alreadyHasRole)
            {
                dbContext.Set<AccountRole>().Add(new AccountRole { AccountId = accountId, RoleId = role.Id });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
