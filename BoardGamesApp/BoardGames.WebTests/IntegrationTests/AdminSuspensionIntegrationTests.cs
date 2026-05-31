using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public class AdminSuspensionIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public AdminSuspensionIntegrationTests(SharedDatabaseFixture fixture)
        {
            this._fixture = fixture;
        }

        private async Task<(AdminController, AuthController, AppDbContext)> CreateTestSubjects(IServiceScope scope)
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();

            // Setup real repositories
            var accountRepo = new AccountRepository(dbContextFactory);
            var failedLoginRepo = new FailedLoginRepository(dbContextFactory);
            
            // Setup AdminController
            var adminService = new AdminService(accountRepo, failedLoginRepo);
            var adminController = new AdminController(adminService);

            // Setup AuthController to test access invalidation (cannot initiate new session)
            var authService = new AuthService(accountRepo, failedLoginRepo);
            var authController = new AuthController(authService);
            
            // Mock HttpContext for AuthController (SignInAsync uses IAuthenticationService)
            var authServiceMock = new Mock<IAuthenticationService>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return (adminController, authController, dbContext);
        }

        [Fact]
        public async Task SuspendAccount_TogglesSuspendedStateToTrue_AndBlocksLogin()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (adminController, authController, dbContext) = await this.CreateTestSubjects(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"active_user_{uniqueId}";
            var password = "SecurePassword123!";
            var random = new Random();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword(password),
                DisplayName = "Active User",
                Country = "Test",
                City = "Test",
                IsSuspended = false // Active initially
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Pre-condition: Login should work
            var loginDto = new LoginDTO { UsernameOrEmail = username, Password = password };
            var preLoginResult = await authController.Login(loginDto);
            Assert.IsType<OkObjectResult>(preLoginResult.Result);

            // Act: Suspend the account via AdminController
            var suspendResult = await adminController.Suspend(user.Id);

            // Assert
            Assert.IsType<NoContentResult>(suspendResult);

            // 1. Verify DB persistence
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            var updatedUser = await finalDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
            
            Assert.NotNull(updatedUser);
            Assert.True(updatedUser.IsSuspended, "The IsSuspended flag should be true in the database.");

            // 2. Verify Application Access Invalidation (Cannot initiate new session)
            var postLoginResult = await authController.Login(loginDto);
            var forbiddenResult = Assert.IsType<ObjectResult>(postLoginResult.Result);
            
            Assert.Equal(403, forbiddenResult.StatusCode);
            var errorResponse = Assert.IsType<ApiErrorResponse>(forbiddenResult.Value);
            Assert.Contains("suspended", errorResponse.Error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UnsuspendAccount_TogglesSuspendedStateToFalse_AndRestoresLogin()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (adminController, authController, dbContext) = await this.CreateTestSubjects(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"suspended_user_{uniqueId}";
            var password = "SecurePassword123!";
            var random = new Random();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword(password),
                DisplayName = "Suspended User",
                Country = "Test",
                City = "Test",
                IsSuspended = true // Suspended initially
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Pre-condition: Login should fail initially
            var loginDto = new LoginDTO { UsernameOrEmail = username, Password = password };
            var preLoginResult = await authController.Login(loginDto);
            Assert.Equal(403, (preLoginResult.Result as ObjectResult)?.StatusCode);

            // Act: Unsuspend the account via AdminController
            var unsuspendResult = await adminController.Unsuspend(user.Id);

            // Assert
            Assert.IsType<NoContentResult>(unsuspendResult);

            // 1. Verify DB persistence
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            var updatedUser = await finalDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
            
            Assert.NotNull(updatedUser);
            Assert.False(updatedUser.IsSuspended, "The IsSuspended flag should be false in the database.");

            // 2. Verify Application Access is Restored
            var postLoginResult = await authController.Login(loginDto);
            Assert.IsType<OkObjectResult>(postLoginResult.Result);
        }
    }
}
