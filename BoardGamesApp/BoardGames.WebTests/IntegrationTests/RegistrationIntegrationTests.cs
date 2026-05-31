using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Api.Controllers;
using BoardGames.Api.Services;
using BoardGames.Api.Security;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Common;

namespace BoardGames.WebTests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class RegistrationIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public RegistrationIntegrationTests(SharedDatabaseFixture fixture)
        {
            this._fixture = fixture;
        }

        private async Task<(AuthController, AppDbContext)> CreateTestSubject(IServiceScope scope)
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();

            // Seed the required "Standard User" role if it doesn't exist
            if (!await dbContext.Roles.AnyAsync(r => r.Name == "Standard User"))
            {
                dbContext.Roles.Add(new Role { Name = "Standard User" });
                await dbContext.SaveChangesAsync();
            }

            var accountRepo = new AccountRepository(dbContextFactory);
            var failedLoginRepo = new FailedLoginRepository(dbContextFactory);
            
            var authService = new AuthService(accountRepo, failedLoginRepo);
            var controller = new AuthController(authService);

            return (controller, dbContext);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, _) = await this.CreateTestSubject(scope);

            var randomStr = Guid.NewGuid().ToString("N").Substring(0, 8);
            var request = new RegisterDTO
            {
                Username = $"weak_user_{randomStr}",
                Email = $"weak_{randomStr}@test.com",
                DisplayName = "Weak Password Test",
                Password = "123", // Weak password
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequest = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var errorResponse = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            
            // PasswordValidator typically requires length and complexity
            Assert.Contains("Password", errorResponse.Error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsConflict()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext) = await this.CreateTestSubject(scope);

            var random = new Random();
            var username = "taken_user_" + Guid.NewGuid().ToString("N");

            // Manually insert an existing user with this username
            var existingUser = new User 
            { 
                Id = Guid.NewGuid(), 
                PamUserId = random.Next(1000000, 9999999), 
                Username = username, 
                Email = "taken@test.com", 
                PasswordHash = "hash", 
                Country = "Test", 
                City = "Test" 
            };
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();

            var request = new RegisterDTO
            {
                Username = username, // Same username
                Email = "different@test.com",
                DisplayName = "Duplicate Tester",
                Password = "StrongPassword123!",
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode); // 409 Conflict
            var errorResponse = Assert.IsType<ApiErrorResponse>(conflictResult.Value);
            Assert.Contains("Username is already taken", errorResponse.Error);
        }

        [Fact]
        public async Task Register_ValidData_CreatesUserWithHashedPasswordAndStandardRole()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext) = await this.CreateTestSubject(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var request = new RegisterDTO
            {
                Username = $"new_user_{uniqueId}",
                Email = $"new_user_{uniqueId}@test.com",
                DisplayName = "New User Test",
                Password = "SuperSecretPassword123!",
                Country = "USA",
                City = "Seattle",
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verify Database State using a fresh context
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            
            var savedUser = await finalDb.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            Assert.NotNull(savedUser);
            Assert.Equal(request.Email, savedUser.Email);
            Assert.Equal(request.DisplayName, savedUser.DisplayName);

            // 1. Verify Password Hash is generated (should not be plain text)
            Assert.NotEqual(request.Password, savedUser.PasswordHash);
            Assert.True(PasswordHasher.VerifyPassword(request.Password, savedUser.PasswordHash));

            // 2. Verify Standard Role is mapped via AccountRoles
            var standardRole = await finalDb.Roles.FirstOrDefaultAsync(r => r.Name == "Standard User");
            Assert.NotNull(standardRole);

            bool hasStandardRole = await finalDb.AccountRoles
                .AnyAsync(ar => ar.AccountId == savedUser.Id && ar.RoleId == standardRole.Id);
            Assert.True(hasStandardRole, "User should be assigned the 'Standard User' role upon registration.");
        }
    }
}
