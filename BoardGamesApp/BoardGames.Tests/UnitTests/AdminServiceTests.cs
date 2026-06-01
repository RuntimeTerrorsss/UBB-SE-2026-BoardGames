using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Security;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Moq;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class AdminServiceTests
    {
        private readonly Mock<IAccountRepository> mockAccountRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly AdminService adminService;

        public AdminServiceTests()
        {
            this.mockAccountRepository = new Mock<IAccountRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.adminService = new AdminService(
                this.mockAccountRepository.Object,
                this.mockFailedLoginRepository.Object);
        }

        #region SuspendAccountAsync

        [Fact]
        public async Task SuspendAccountAsync_AccountNotFound_ReturnsFailure()
        {
            var accountId = Guid.NewGuid();

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync((User)null);

            var result = await this.adminService.SuspendAccountAsync(accountId);

            Assert.False(result.Success);
            Assert.Equal("Account not found.", result.Error);
        }

        [Fact]
        public async Task SuspendAccountAsync_ValidAccount_SetsIsSuspendedTrue()
        {
            var accountId = Guid.NewGuid();
            var account = new User { Id = accountId, IsSuspended = false };

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            this.mockAccountRepository
                .Setup(repo => repo.UpdateAsync(account))
                .Returns(Task.CompletedTask);

            var result = await this.adminService.SuspendAccountAsync(accountId);

            Assert.True(result.Success);
            Assert.True(account.IsSuspended);
        }

        #endregion

        #region UnsuspendAccountAsync

        [Fact]
        public async Task UnsuspendAccountAsync_AccountNotFound_ReturnsFailure()
        {
            var accountId = Guid.NewGuid();

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync((User)null);

            var result = await this.adminService.UnsuspendAccountAsync(accountId);

            Assert.False(result.Success);
            Assert.Equal("Account not found.", result.Error);
        }

        [Fact]
        public async Task UnsuspendAccountAsync_ValidAccount_SetsIsSuspendedFalse()
        {
            var accountId = Guid.NewGuid();
            var account = new User { Id = accountId, IsSuspended = true };

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            this.mockAccountRepository
                .Setup(repo => repo.UpdateAsync(account))
                .Returns(Task.CompletedTask);

            var result = await this.adminService.UnsuspendAccountAsync(accountId);

            Assert.True(result.Success);
            Assert.False(account.IsSuspended);
        }

        #endregion

        #region UnlockAccountAsync

        [Fact]
        public async Task UnlockAccountAsync_WhenCalled_CallsResetOnFailedLoginRepository()
        {
            var accountId = Guid.NewGuid();

            this.mockFailedLoginRepository
                .Setup(repo => repo.ResetAsync(accountId))
                .Returns(Task.CompletedTask);

            var result = await this.adminService.UnlockAccountAsync(accountId);

            Assert.True(result.Success);
            this.mockFailedLoginRepository.Verify(repo => repo.ResetAsync(accountId), Times.Once);
        }

        #endregion

        #region ResetPasswordAsync

        [Fact]
        public async Task ResetPasswordAsync_AccountNotFound_ReturnsFailure()
        {
            var accountId = Guid.NewGuid();

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync((User)null);

            var result = await this.adminService.ResetPasswordAsync(accountId, "Valid1!");

            Assert.False(result.Success);
            Assert.Equal("Account not found.", result.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidPassword_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            var account = new User { Id = accountId, PasswordHash = "oldhash" };

            this.mockAccountRepository
                .Setup(repo => repo.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            this.mockAccountRepository
                .Setup(repo => repo.UpdateAsync(account))
                .Returns(Task.CompletedTask);

            var result = await this.adminService.ResetPasswordAsync(accountId, "Valid1!");

            Assert.True(result.Success);
            Assert.NotEqual("oldhash", account.PasswordHash);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidPassword_ReturnsFailure()
        {
            var accountId = Guid.NewGuid();
            var result = await this.adminService.ResetPasswordAsync(accountId, "short");

            Assert.False(result.Success);
            Assert.Contains("invalid", result.Error.ToLower());
        }

        #endregion

        #region GetAllAccountsAsync

        [Fact]
        public async Task GetAllAccountsAsync_ValidRequest_ReturnsAccountList()
        {
            var accounts = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1", DisplayName = "User One", Email = "u1@email.com" },
                new User { Id = Guid.NewGuid(), Username = "user2", DisplayName = "User Two", Email = "u2@email.com" },
            };

            this.mockAccountRepository
                .Setup(repo => repo.GetAllAsync(1, 100))
                .ReturnsAsync(accounts);

            this.mockFailedLoginRepository
                .Setup(repo => repo.GetByAccountIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((FailedLoginAttempt)null);

            var result = await this.adminService.GetAllAccountsAsync(1, 100);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion
    }
}