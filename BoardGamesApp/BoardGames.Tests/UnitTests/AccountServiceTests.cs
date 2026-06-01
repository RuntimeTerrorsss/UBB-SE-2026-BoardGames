//using System;
//using System.Threading.Tasks;
//using BoardGames.Api.Mappers;
//using BoardGames.Api.Services;
//using BoardGames.Data.Models;
//using BoardGames.Data.Repositories;
//using BoardGames.Shared.DTO;
//using Moq;
//using Xunit;

//namespace BoardGames.Tests.UnitTests
//{
//    public class AccountServiceTests
//    {
//        private readonly Mock<IAccountRepository> mockAccountRepository;
//        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
//        private readonly Mock<IAvatarStorageService> mockAvatarStorageService;
//        private readonly AccountProfileMapper mapper;
//        private readonly AccountService accountService;

//        public AccountServiceTests()
//        {
//            this.mockAccountRepository = new Mock<IAccountRepository>();
//            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
//            this.mockAvatarStorageService = new Mock<IAvatarStorageService>();
//            this.mapper = new AccountProfileMapper();
//            this.accountService = new AccountService(
//                this.mockAccountRepository.Object,
//                this.mapper,
//                this.mockAvatarStorageService.Object,
//                this.mockFailedLoginRepository.Object);
//        }

//        #region GetProfileAsync

//        [Fact]
//        public async Task GetProfileAsync_AccountNotFound_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync((User)null);

//            var result = await this.accountService.GetProfileAsync(accountId);

//            Assert.False(result.Success);
//            Assert.Equal("Account not found.", result.Error);
//        }

//        [Fact]
//        public async Task GetProfileAsync_AccountExists_ReturnsProfile()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User
//            {
//                Id = accountId,
//                Username = "testuser",
//                DisplayName = "Test User",
//                Email = "test@email.com",
//                IsSuspended = false,
//            };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockFailedLoginRepository
//                .Setup(repo => repo.GetByAccountIdAsync(accountId))
//                .ReturnsAsync((FailedLoginAttempt)null);

//            var result = await this.accountService.GetProfileAsync(accountId);

//            Assert.True(result.Success);
//            Assert.Equal(accountId, result.Data.Id);
//            Assert.Equal("testuser", result.Data.Username);
//            Assert.False(result.Data.IsLocked);
//        }

//        [Fact]
//        public async Task GetProfileAsync_AccountIsLocked_ReturnsIsLockedTrue()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId, Username = "lockeduser" };
//            var failedAttempt = new FailedLoginAttempt
//            {
//                AccountId = accountId,
//                LockedUntil = DateTime.UtcNow.AddMinutes(10),
//            };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockFailedLoginRepository
//                .Setup(repo => repo.GetByAccountIdAsync(accountId))
//                .ReturnsAsync(failedAttempt);

//            var result = await this.accountService.GetProfileAsync(accountId);

//            Assert.True(result.Success);
//            Assert.True(result.Data.IsLocked);
//        }

//        #endregion

//        #region UpdateProfileAsync

//        [Fact]
//        public async Task UpdateProfileAsync_AccountNotFound_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync((User)null);

//            var result = await this.accountService.UpdateProfileAsync(accountId, new AccountProfileDTO { DisplayName = "Test" });

//            Assert.False(result.Success);
//            Assert.Equal("Account not found.", result.Error);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_InvalidDisplayName_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            var result = await this.accountService.UpdateProfileAsync(accountId, new AccountProfileDTO { DisplayName = "X" });

//            Assert.False(result.Success);
//            Assert.Contains("DisplayName", result.Error);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_ValidData_ReturnsSuccess()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId, Email = "old@email.com" };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockAccountRepository
//                .Setup(repo => repo.UpdateAsync(account))
//                .Returns(Task.CompletedTask);

//            var result = await this.accountService.UpdateProfileAsync(accountId, new AccountProfileDTO
//            {
//                DisplayName = "Valid Name",
//                Email = "old@email.com",
//            });

//            Assert.True(result.Success);
//        }

//        #endregion

//        #region ChangePasswordAsync

//        [Fact]
//        public async Task ChangePasswordAsync_AccountNotFound_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync((User)null);

//            var result = await this.accountService.ChangePasswordAsync(accountId, "old", "new");

//            Assert.False(result.Success);
//            Assert.Equal("Account not found.", result.Error);
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId, PasswordHash = "somehashedvalue" };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            var result = await this.accountService.ChangePasswordAsync(accountId, "wrongpassword", "newpass");

//            Assert.False(result.Success);
//            Assert.Equal("Current password is incorrect.", result.Error);
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_ValidPasswords_ReturnsSuccess()
//        {
//            var accountId = Guid.NewGuid();
//            string currentPassword = "OldPassword123!";
//            string newPassword = "NewValidPassword123!";

//            string hashedCurrentPassword = BoardGames.Api.Security.PasswordHasher.HashPassword(currentPassword);

//            var account = new User { Id = accountId, PasswordHash = hashedCurrentPassword };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockAccountRepository
//                .Setup(repo => repo.UpdateAsync(account))
//                .Returns(Task.CompletedTask);

//            var result = await this.accountService.ChangePasswordAsync(accountId, currentPassword, newPassword);

//            Assert.True(result.Success);

//            this.mockAccountRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Id == accountId)), Times.Once);
//        }

//        #endregion

//        #region RemoveAvatarAsync

//        [Fact]
//        public async Task RemoveAvatarAsync_AccountNotFound_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync((User)null);

//            var result = await this.accountService.RemoveAvatarAsync(accountId);

//            Assert.False(result.Success);
//            Assert.Equal("Account not found.", result.Error);
//        }

//        [Fact]
//        public async Task RemoveAvatarAsync_HasAvatar_DeletesAndClears()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId, AvatarUrl = "avatars/test.png" };

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockAccountRepository
//                .Setup(repo => repo.UpdateAsync(account))
//                .Returns(Task.CompletedTask);

//            var result = await this.accountService.RemoveAvatarAsync(accountId);

//            Assert.True(result.Success);
//            this.mockAvatarStorageService.Verify(s => s.Delete("avatars/test.png"), Times.Once);
//        }

//        #endregion

//        #region SetAvatarUrlAsync

//        [Fact]
//        public async Task SetAvatarUrlAsync_AccountNotFound_ReturnsFailure()
//        {
//            var accountId = Guid.NewGuid();

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync((User)null);

//            var result = await this.accountService.SetAvatarUrlAsync(accountId, "avatars/new.png");

//            Assert.False(result.Success);
//            Assert.Equal("Account not found.", result.Error);
//        }

//        [Fact]
//        public async Task SetAvatarUrlAsync_ValidData_ReturnsSuccess()
//        {
//            var accountId = Guid.NewGuid();
//            var account = new User { Id = accountId, AvatarUrl = string.Empty };
//            string newAvatarUrl = "avatars/new.png";

//            this.mockAccountRepository
//                .Setup(repo => repo.GetByIdAsync(accountId))
//                .ReturnsAsync(account);

//            this.mockAccountRepository
//                .Setup(repo => repo.UpdateAsync(account))
//                .Returns(Task.CompletedTask);

//            var result = await this.accountService.SetAvatarUrlAsync(accountId, newAvatarUrl);

//            Assert.True(result.Success);
//            Assert.Equal(newAvatarUrl, result.Data);
//            Assert.Equal(newAvatarUrl, account.AvatarUrl);

//            this.mockAccountRepository.Verify(repo => repo.UpdateAsync(account), Times.Once);
//        }

//        #endregion
//    }
//}
