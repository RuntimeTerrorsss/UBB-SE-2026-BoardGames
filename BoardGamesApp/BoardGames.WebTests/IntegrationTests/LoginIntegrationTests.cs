using System;
using System.Linq;
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
    public class LoginIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public LoginIntegrationTests(SharedDatabaseFixture fixture)
        {
            this._fixture = fixture;
        }

        private async Task<(AuthController, AppDbContext, Mock<IAuthenticationService>)> CreateTestSubject(IServiceScope scope)
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();

            // Setup real repositories and service
            var accountRepo = new AccountRepository(dbContextFactory);
            var failedLoginRepo = new FailedLoginRepository(dbContextFactory);
            var authService = new AuthService(accountRepo, failedLoginRepo);
            
            var controller = new AuthController(authService);

            // Mock IAuthenticationService for HttpContext.SignInAsync
            var authServiceMock = new Mock<IAuthenticationService>();
            
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return (controller, dbContext, authServiceMock);
        }

        [Fact]
        public async Task Login_ValidCredentials_SignsInAndReturnsProfile()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, authServiceMock) = await this.CreateTestSubject(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"valid_user_{uniqueId}";
            var password = "SecurePassword123!";
            var passwordHash = PasswordHasher.HashPassword(password);
            var random = new Random();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = passwordHash,
                DisplayName = "Valid Login User",
                Country = "Test",
                City = "Test",
                IsSuspended = false
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var loginDto = new LoginDTO
            {
                UsernameOrEmail = username,
                Password = password
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var profile = Assert.IsType<AccountProfileDTO>(okResult.Value);
            
            Assert.Equal(user.Id, profile.Id);
            Assert.Equal(user.Username, profile.Username);

            // Verify SignInAsync was called and the ClaimsPrincipal contains correct claims
            authServiceMock.Verify(a => a.SignInAsync(
                It.IsAny<HttpContext>(), 
                It.Is<string>(s => s == "Cookies"), 
                It.Is<ClaimsPrincipal>(cp => 
                    cp.HasClaim(ClaimTypes.NameIdentifier, user.Id.ToString()) &&
                    cp.HasClaim(ClaimTypes.Name, user.Username)
                ), 
                It.IsAny<AuthenticationProperties>()), 
            Times.Once);
        }

        [Fact]
        public async Task Login_SuspendedAccount_ReturnsForbidden()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, _) = await this.CreateTestSubject(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"suspended_user_{uniqueId}";
            var random = new Random();

            var suspendedUser = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword("any_password"),
                DisplayName = "Suspended User",
                Country = "Test",
                City = "Test",
                IsSuspended = true // Blocked!
            };
            dbContext.Users.Add(suspendedUser);
            await dbContext.SaveChangesAsync();

            var loginDto = new LoginDTO
            {
                UsernameOrEmail = username,
                Password = "any_password"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, forbiddenResult.StatusCode); // 403 Forbidden
            var errorResponse = Assert.IsType<ApiErrorResponse>(forbiddenResult.Value);
            Assert.Contains("suspended", errorResponse.Error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Login_LockedAccount_ReturnsForbidden()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, _) = await this.CreateTestSubject(scope);

            var uniqueId = Guid.NewGuid().ToString("N");
            var username = $"locked_user_{uniqueId}";
            var random = new Random();

            var lockedUser = new User
            {
                Id = Guid.NewGuid(),
                PamUserId = random.Next(1000000, 9999999),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = PasswordHasher.HashPassword("valid_password"),
                DisplayName = "Locked User",
                Country = "Test",
                City = "Test",
                IsSuspended = false
            };
            dbContext.Users.Add(lockedUser);
            
            var failedLogin = new FailedLoginAttempt
            {
                AccountId = lockedUser.Id,
                FailedAttempts = 5,
                LockedUntil = DateTime.UtcNow.AddMinutes(15) // Currently Locked
            };
            dbContext.FailedLoginAttempts.Add(failedLogin);
            await dbContext.SaveChangesAsync();

            var loginDto = new LoginDTO
            {
                UsernameOrEmail = username,
                Password = "valid_password"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, forbiddenResult.StatusCode); // 403 Forbidden
            var errorResponse = Assert.IsType<ApiErrorResponse>(forbiddenResult.Value);
            Assert.Contains("locked", errorResponse.Error, StringComparison.OrdinalIgnoreCase);
        }
    }
}
