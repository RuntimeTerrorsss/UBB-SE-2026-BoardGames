using System;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Moq;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IAccountRepository> mockAccountRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly AuthService authService;

        public AuthServiceTests()
        {
            this.mockAccountRepository = new Mock<IAccountRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.authService = new AuthService(
                this.mockAccountRepository.Object,
                this.mockFailedLoginRepository.Object);
        }

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyTaken_ReturnsFailure()
        {
            var request = new RegisterDTO
            {
                Username = "existinguser",
                Email = "new@email.com",
                Password = "Valid1!",
                DisplayName = "Test",
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync(new User { Username = request.Username });

            var result = await this.authService.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Contains("Username", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyRegistered_ReturnsFailure()
        {
            var request = new RegisterDTO
            {
                Username = "newuser",
                Email = "taken@email.com",
                Password = "Valid1!",
                DisplayName = "Test",
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User)null);

            this.mockAccountRepository
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });

            var result = await this.authService.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Contains("Email", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
        {
            var request = new RegisterDTO
            {
                Username = "newuser",
                Email = "new@email.com",
                Password = "Valid1!",
                DisplayName = "Test",
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User)null);

            this.mockAccountRepository
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            this.mockAccountRepository
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            this.mockAccountRepository
                .Setup(repo => repo.AddRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await this.authService.RegisterAsync(request);

            Assert.True(result.Success);
        }

        #endregion

        #region LoginAsync

        [Fact]
        public async Task LoginAsync_AccountNotFound_ReturnsFailure()
        {
            var request = new LoginDTO { UsernameOrEmail = "nobody", Password = "pass" };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.UsernameOrEmail))
                .ReturnsAsync((User)null);

            this.mockAccountRepository
                .Setup(repo => repo.GetByEmailAsync(request.UsernameOrEmail))
                .ReturnsAsync((User)null);

            var result = await this.authService.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Error);
        }

        [Fact]
        public async Task LoginAsync_AccountIsSuspended_ReturnsFailure()
        {
            var request = new LoginDTO { UsernameOrEmail = "suspended", Password = "pass" };
            var account = new User { Username = "suspended", IsSuspended = true };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.UsernameOrEmail))
                .ReturnsAsync(account);

            var result = await this.authService.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Contains("suspended", result.Error);
        }

        [Fact]
        public async Task LoginAsync_AccountIsLocked_ReturnsFailure()
        {
            var request = new LoginDTO { UsernameOrEmail = "locked", Password = "pass" };
            var account = new User { Username = "locked", IsSuspended = false };
            var failedAttempt = new FailedLoginAttempt
            {
                AccountId = account.Id,
                LockedUntil = DateTime.UtcNow.AddMinutes(10),
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.UsernameOrEmail))
                .ReturnsAsync(account);

            this.mockFailedLoginRepository
                .Setup(repo => repo.GetByAccountIdAsync(account.Id))
                .ReturnsAsync(failedAttempt);

            var result = await this.authService.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Contains("locked", result.Error);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsFailure()
        {
            var request = new LoginDTO { UsernameOrEmail = "user", Password = "wrongpass" };
            var account = new User
            {
                Username = "user",
                IsSuspended = false,
                PasswordHash = "somehashedvalue",
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.UsernameOrEmail))
                .ReturnsAsync(account);

            this.mockFailedLoginRepository
                .Setup(repo => repo.GetByAccountIdAsync(account.Id))
                .ReturnsAsync((FailedLoginAttempt)null);

            this.mockFailedLoginRepository
                .Setup(repo => repo.IncrementAsync(account.Id))
                .Returns(Task.CompletedTask);

            var result = await this.authService.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Error);
            this.mockFailedLoginRepository.Verify(repo => repo.IncrementAsync(account.Id), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsProfile()
        {
            var request = new LoginDTO { UsernameOrEmail = "user", Password = "ValidPassword123!" };

            string hashedPassword = BoardGames.Api.Security.PasswordHasher.HashPassword(request.Password);

            var account = new User
            {
                Id = Guid.NewGuid(),
                Username = "user",
                IsSuspended = false,
                PasswordHash = hashedPassword
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetByUsernameAsync(request.UsernameOrEmail))
                .ReturnsAsync(account);

            this.mockFailedLoginRepository
                .Setup(repo => repo.GetByAccountIdAsync(account.Id))
                .ReturnsAsync((FailedLoginAttempt)null);

            this.mockFailedLoginRepository
                .Setup(repo => repo.ResetAsync(account.Id))
                .Returns(Task.CompletedTask);

            var result = await this.authService.LoginAsync(request);

            Assert.True(result.Success);
            Assert.Equal("user", result.Data.Username);
            Assert.False(result.Data.IsLocked);

            this.mockFailedLoginRepository.Verify(repo => repo.ResetAsync(account.Id), Times.Once);
        }

        #endregion

        #region LogoutAsync

        [Fact]
        public async Task LogoutAsync_WhenCalled_AlwaysReturnsSuccess()
        {
            var result = await this.authService.LogoutAsync();
            Assert.True(result.Success);
        }

        #endregion

        #region ForgotPasswordAsync

        [Fact]
        public async Task ForgotPasswordAsync_WhenCalled_ReturnsContactAdminMessage()
        {
            var result = await this.authService.ForgotPasswordAsync();
            Assert.True(result.Success);
            Assert.Contains("admin@boardrent.com", result.Data);
        }

        #endregion

    }
}