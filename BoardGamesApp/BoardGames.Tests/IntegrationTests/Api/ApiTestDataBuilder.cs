using System;
using System.Threading.Tasks;
using BoardGames.Api.Security;
using BoardGames.Data;
using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace BoardGames.Tests.IntegrationTests.Api
{
    public static class ApiTestDataBuilder
    {
        public static async Task<Guid> SeedUserAsync(AppDbContext dbContext, Guid accountId, int pamUserId, string username, string email, bool isAdmin = false)
        {
            var user = new User
            {
                Id = accountId,
                PamUserId = pamUserId,
                Username = username,
                DisplayName = username,
                Email = email,
                PasswordHash = PasswordHasher.HashPassword("Password123!"),
                PhoneNumber = string.Empty,
                AvatarUrl = string.Empty,
                Country = string.Empty,
                City = "Cluj",
                StreetName = "Main",
                StreetNumber = "1",
                IsSuspended = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            dbContext.Users.Add(user);

            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == (isAdmin ? "Administrator" : "Standard User"));
            if (role != null)
            {
                dbContext.AccountRoles.Add(new AccountRole { AccountId = accountId, RoleId = role.Id });
            }

            await dbContext.SaveChangesAsync();
            return user.Id;
        }

        public static async Task<int> SeedGameAsync(AppDbContext dbContext, int ownerPamUserId, string name = "Integration Game")
        {
            var game = new Game
            {
                Name = name,
                OwnerId = ownerPamUserId,
                PricePerDay = 12m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "Integration test game",
                Image = Array.Empty<byte>(),
                IsActive = true,
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();
            return game.Id;
        }
    }
}