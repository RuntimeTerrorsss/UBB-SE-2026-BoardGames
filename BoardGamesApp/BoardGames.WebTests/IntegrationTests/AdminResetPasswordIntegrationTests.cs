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
    public class AdminResetPasswordIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public AdminResetPasswordIntegrationTests(SharedDatabaseFixture fixture)
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

            // Setup AuthController to test authentication invalidation
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
        public async Task ResetPassword_WeakPassword_ReturnsBadRequest()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (adminController, _, dbContext) = await this.CreateTestSubjects(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"weak_reset_{uniqueId}";
            var random = new Random();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword("OldStrongPassword123!"),
                DisplayName = "Weak Reset User",
                Country = "Test",
                City = "Test",
                IsSuspended = false
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var weakPasswordBody = new ResetPasswordDTO { NewPassword = "123" };

            // Act
            var resetResult = await adminController.ResetPassword(user.Id, weakPasswordBody);

            // Assert
            var badRequest = Assert.IsType<ObjectResult>(resetResult);
            Assert.Equal(400, badRequest.StatusCode);
            var errorResponse = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            
            // Validate the policy check rejected the short password
            Assert.Contains("Password", errorResponse.Error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ResetPassword_ValidPassword_ReplacesHashAndAuthenticates()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (adminController, authController, dbContext) = await this.CreateTestSubjects(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"reset_user_{uniqueId}";
            var oldPassword = "OldStrongPassword123!";
            var newPassword = "NewStrongPassword456!";
            var random = new Random();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword(oldPassword),
                DisplayName = "Reset User",
                Country = "Test",
                City = "Test",
                IsSuspended = false
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Pre-condition: Login works with OLD password
            var loginDtoOld = new LoginDTO { UsernameOrEmail = username, Password = oldPassword };
            var preLoginResult = await authController.Login(loginDtoOld);
            Assert.IsType<OkObjectResult>(preLoginResult.Result);

            var resetPasswordBody = new ResetPasswordDTO { NewPassword = newPassword };

            // Act: Admin resets the password
            var resetResult = await adminController.ResetPassword(user.Id, resetPasswordBody);

            // Assert
            Assert.IsType<NoContentResult>(resetResult);

            // 1. Verify DB persistence (Hash is overridden)
            var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            var updatedUser = await finalDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
            
            Assert.NotNull(updatedUser);
            Assert.NotEqual(PasswordHasher.HashPassword(oldPassword), updatedUser.PasswordHash);
            Assert.True(PasswordHasher.VerifyPassword(newPassword, updatedUser.PasswordHash), "The database should contain a valid hash of the NEW password.");

            // 2. Verify Application Access is Invalidated for OLD password
            var postLoginOldResult = await authController.Login(loginDtoOld);
            var unauthorizedResult = Assert.IsType<ObjectResult>(postLoginOldResult.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode); // 401 Unauthorized because credentials are bad

            // 3. Verify Application Access works for NEW password
            var loginDtoNew = new LoginDTO { UsernameOrEmail = username, Password = newPassword };
            var postLoginNewResult = await authController.Login(loginDtoNew);
            Assert.IsType<OkObjectResult>(postLoginNewResult.Result); // 200 OK
        }
    }
}
