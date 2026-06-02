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
    public class AdminUnlockIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public AdminUnlockIntegrationTests(SharedDatabaseFixture fixture)
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

            // Setup AuthController to test access restoration
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
        public async Task UnlockAccount_ResetsFailedLoginAttempt_AndRestoresLogin()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (adminController, authController, dbContext) = await this.CreateTestSubjects(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"locked_user_{uniqueId}";
            var password = "SecurePassword123!";
            var random = new Random();

            // 1. Create User
            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword(password),
                DisplayName = "Locked User",
                Country = "Test",
                City = "Test",
                IsSuspended = false
            };
            dbContext.Users.Add(user);
            
            // 2. Create Locked Attempt
            var failedLogin = new FailedLoginAttempt
            {
                AccountId = user.Id,
                FailedAttempts = 5,
                LockedUntil = DateTime.UtcNow.AddMinutes(30) // Locked in the future
            };
            dbContext.FailedLoginAttempts.Add(failedLogin);
            await dbContext.SaveChangesAsync();

            // Pre-condition: Login should fail immediately due to lock
            var loginDto = new LoginDTO { UsernameOrEmail = username, Password = password };
            var preLoginResult = await authController.Login(loginDto);
            
            var preLoginForbidden = Assert.IsType<ObjectResult>(preLoginResult.Result);
            Assert.Equal(403, preLoginForbidden.StatusCode); // 403 Forbidden

            // Act: Admin triggers Unlock
            var unlockResult = await adminController.Unlock(user.Id);

            // Assert
            Assert.IsType<NoContentResult>(unlockResult);

            // 1. Verify DB persistence cleanup (wiped timestamps)
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            var updatedFailedAttempt = await finalDb.FailedLoginAttempts.AsNoTracking().FirstOrDefaultAsync(f => f.AccountId == user.Id);
            
            Assert.NotNull(updatedFailedAttempt);
            Assert.Equal(0, updatedFailedAttempt.FailedAttempts);
            Assert.Null(updatedFailedAttempt.LockedUntil);

            // 2. Verify Application Access is Restored
            var postLoginResult = await authController.Login(loginDto);
            Assert.IsType<OkObjectResult>(postLoginResult.Result);
        }
    }
}
