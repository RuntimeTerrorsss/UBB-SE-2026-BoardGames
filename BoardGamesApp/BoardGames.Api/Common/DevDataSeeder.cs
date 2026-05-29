// <copyright file="DevDataSeeder.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BoardGames.Api.Security;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoardGames.Api.Common
{
    public static class DevDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            if (!environment.IsDevelopment())
            {
                return;
            }

            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DevDataSeeder");

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var gamesRepository = scope.ServiceProvider.GetRequiredService<InterfaceGamesRepository>();

            await dbContext.Database.MigrateAsync();

            await EnsureRoleAsync(dbContext, "Standard User");
            await EnsureRoleAsync(dbContext, "Administrator");
            await EnsureRoleAsync(dbContext, "Admin");

            await EnsureAccountAsync(
                accountRepository,
                username: "admin",
                displayName: "Administrator",
                email: "admin@boardrent.com",
                pamUserId: 4,
                password: "Password123!",
                roleNames: new[] { "Administrator", "Admin" });

            await EnsureAccountAsync(
                accountRepository,
                username: "mihai",
                displayName: "Mihai",
                email: "mihai@boardrent.com",
                pamUserId: 2,
                password: "Password123!",
                roleNames: new[] { "Standard User" });

            await EnsureSampleGamesAsync(dbContext, accountRepository, gamesRepository);

            logger.LogInformation("Development seed ensured: admin/mihai accounts present.");
        }

        private static async Task EnsureSampleGamesAsync(
            AppDbContext dbContext,
            IAccountRepository accountRepository,
            InterfaceGamesRepository gamesRepository)
        {
            var admin = await accountRepository.GetByUsernameAsync("admin");
            var mihai = await accountRepository.GetByUsernameAsync("mihai");

            if (admin == null || mihai == null)
            {
                return;
            }

            await EnsureGameAsync(dbContext, gamesRepository,
                name: "Catan",
                pricePerDay: 1.99m,
                minimumPlayers: 3,
                maximumPlayers: 4,
                description: "Trade and build on the island of Catan.",
                ownerAccountId: admin.Id,
                isActive: true);

            await EnsureGameAsync(dbContext, gamesRepository,
                name: "Carcassonne",
                pricePerDay: 1.20m,
                minimumPlayers: 2,
                maximumPlayers: 5,
                description: "Tile placement game with medieval cities.",
                ownerAccountId: admin.Id,
                isActive: true);

            await EnsureGameAsync(dbContext, gamesRepository,
                name: "Terraforming Mars",
                pricePerDay: 2.50m,
                minimumPlayers: 1,
                maximumPlayers: 5,
                description: "Develop Mars with corporations and projects.",
                ownerAccountId: mihai.Id,
                isActive: true);

            await EnsureGameAsync(dbContext, gamesRepository,
                name: "Ticket to Ride",
                pricePerDay: 1.75m,
                minimumPlayers: 2,
                maximumPlayers: 5,
                description: "Collect cards to claim railway routes.",
                ownerAccountId: mihai.Id,
                isActive: true);

            await EnsureGameAsync(dbContext, gamesRepository,
                name: "Chess",
                pricePerDay: 0.86m,
                minimumPlayers: 2,
                maximumPlayers: 2,
                description: "Classic strategy game for two players.",
                ownerAccountId: admin.Id,
                isActive: true);
        }

        private static async Task EnsureGameAsync(
            AppDbContext dbContext,
            InterfaceGamesRepository gamesRepository,
            string name,
            decimal pricePerDay,
            int minimumPlayers,
            int maximumPlayers,
            string description,
            Guid ownerAccountId,
            bool isActive)
        {
            bool exists = await dbContext.Games.AnyAsync(game => game.Name == name);
            if (exists)
            {
                return;
            }

            gamesRepository.AddGame(new Game
            {
                Name = name,
                PricePerDay = pricePerDay,
                MinimumPlayerNumber = minimumPlayers,
                MaximumPlayerNumber = maximumPlayers,
                Description = description,
                Image = null,
                IsActive = isActive,
                Owner = new User { Id = ownerAccountId },
            });
        }

        private static async Task EnsureRoleAsync(AppDbContext dbContext, string roleName)
        {
            bool exists = await dbContext.Roles.AnyAsync(role => role.Name == roleName);
            if (exists)
            {
                return;
            }

            dbContext.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
            });

            await dbContext.SaveChangesAsync();
        }

        private static async Task EnsureAccountAsync(
            IAccountRepository accountRepository,
            string username,
            string displayName,
            string email,
            int pamUserId,
            string password,
            string[] roleNames)
        {
            var account = await accountRepository.GetByUsernameAsync(username)
                          ?? await accountRepository.GetByEmailAsync(email);

            if (account == null)
            {
                account = new User
                {
                    Id = Guid.NewGuid(),
                    PamUserId = pamUserId,
                    Username = username,
                    DisplayName = displayName,
                    Email = email,
                    PasswordHash = PasswordHasher.HashPassword(password),
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    Balance = 0m,
                    IsSuspended = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await accountRepository.AddAsync(account);
            }
            else
            {
                if (!PasswordHasher.VerifyPassword(password, account.PasswordHash))
                {
                    account.PasswordHash = PasswordHasher.HashPassword(password);
                    account.UpdatedAt = DateTime.UtcNow;
                    await accountRepository.UpdateAsync(account);
                }
            }

            foreach (string roleName in roleNames)
            {
                await accountRepository.AddRoleAsync(account.Id, roleName);
            }
        }
    }
}
