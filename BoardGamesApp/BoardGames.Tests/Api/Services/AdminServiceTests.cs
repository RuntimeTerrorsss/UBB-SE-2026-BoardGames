using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using NUnit.Framework;
using AdminService = BoardRentAndProperty.Api.Services.AdminService;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public sealed class AdminServiceTests
    {
        private FakeAccountRepository accountRepository = null!;
        private FakeFailedLoginRepository failedLoginRepository = null!;
        private AdminService service = null!;

        [SetUp]
        public void SetUp()
        {
            accountRepository = new FakeAccountRepository();
            failedLoginRepository = new FakeFailedLoginRepository();
            service = new AdminService(accountRepository, failedLoginRepository);
        }

        [Test]
        public async Task GetAllAccountsAsync_WhenAccountsExist_ReturnsMappedProfiles()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "admin_user",
                DisplayName = "Admin User",
                Email = "admin@test.com",
                IsSuspended = false,
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Administrator" } },
            };

            accountRepository.Accounts = new List<Account> { account };
            failedLoginRepository.FailedLoginAttempts[accountId] = new FailedLoginAttempt
            {
                AccountId = accountId,
                LockedUntil = DateTime.UtcNow.AddMinutes(5),
            };

            var serviceResult = await service.GetAllAccountsAsync(1, 10);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Has.Count.EqualTo(1));
            Assert.That(serviceResult.Data![0].Username, Is.EqualTo("admin_user"));
            Assert.That(serviceResult.Data[0].Role.Name, Is.EqualTo("Administrator"));
            Assert.That(serviceResult.Data[0].IsLocked, Is.True);
        }

        [Test]
        public async Task SuspendAccountAsync_AccountExists_UpdatesStatusToSuspended()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = false };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.SuspendAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.IsSuspended, Is.True);
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task UnsuspendAccountAsync_AccountExists_UpdatesStatusToActive()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, IsSuspended = true };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.UnsuspendAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.IsSuspended, Is.False);
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task ResetPasswordAsync_PasswordTooShort_ReturnsFailResult()
        {
            var serviceResult = await service.ResetPasswordAsync(Guid.NewGuid(), "123");

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.Error, Does.Contain("at least 6 characters"));
        }

        [Test]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = "old_hash";
            var account = new Account { Id = accountId, PasswordHash = originalHash };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.ResetPasswordAsync(accountId, "NewSecurePass123!");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.PasswordHash, Is.Not.EqualTo(originalHash));
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task UnlockAccountAsync_WhenCalled_ResetsFailedAttempts()
        {
            var accountId = Guid.NewGuid();

            var serviceResult = await service.UnlockAccountAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(failedLoginRepository.ResetCallCount, Is.EqualTo(1));
            Assert.That(failedLoginRepository.LastAccountId, Is.EqualTo(accountId));
        }
    }
}
