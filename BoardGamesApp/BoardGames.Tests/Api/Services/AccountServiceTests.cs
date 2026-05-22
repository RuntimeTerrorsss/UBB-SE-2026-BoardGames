using System;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using NUnit.Framework;
using AccountService = BoardRentAndProperty.Api.Services.AccountService;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public sealed class AccountServiceTests
    {
        private FakeAccountRepository accountRepository = null!;
        private FakeAvatarStorageService avatarStorageService = null!;
        private AccountService service = null!;

        [SetUp]
        public void SetUp()
        {
            accountRepository = new FakeAccountRepository();
            avatarStorageService = new FakeAvatarStorageService();
            service = new AccountService(
                accountRepository,
                new AccountProfileMapper(),
                avatarStorageService);
        }

        [Test]
        public async Task GetProfileAsync_AccountDoesNotExist_ReturnsFailResult()
        {
            var accountId = Guid.NewGuid();

            accountRepository.AccountsById[accountId] = null;

            var serviceResult = await service.GetProfileAsync(accountId);

            Assert.That(serviceResult.Success, Is.False);
            Assert.That(serviceResult.Error, Is.EqualTo("Account not found."));
        }

        [Test]
        public async Task GetProfileAsync_AccountExists_ReturnsSuccessResultWithProfileData()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "test_user",
                DisplayName = "Test User Display Name",
                Roles = { new Role { Id = Guid.NewGuid(), Name = "Standard User" } },
            };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.GetProfileAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Is.Not.Null);
            Assert.That(serviceResult.Data!.Username, Is.EqualTo("test_user"));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesAccountAndReturnsSuccess()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                DisplayName = "Original Name",
                Email = "original@test.com",
            };

            var updateData = new AccountProfileDataTransferObject
            {
                DisplayName = "Updated Display Name",
                Email = "updated@test.com",
            };

            accountRepository.AccountsById[accountId] = account;
            accountRepository.AccountsByEmail["updated@test.com"] = null;

            var serviceResult = await service.UpdateProfileAsync(accountId, updateData);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.DisplayName, Is.EqualTo("Updated Display Name"));
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task ChangePasswordAsync_ValidPasswords_UpdatesPasswordHash()
        {
            var accountId = Guid.NewGuid();
            string originalHash = PasswordHasher.HashPassword("OldPassword123!");
            var account = new Account
            {
                Id = accountId,
                PasswordHash = originalHash,
            };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.ChangePasswordAsync(accountId, "OldPassword123!", "NewSecurePass123!");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.PasswordHash, Is.Not.EqualTo(originalHash));
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task SetAvatarUrlAsync_AccountExists_UpdatesAvatarUrl()
        {
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.SetAvatarUrlAsync(accountId, "/avatars/test.png");

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(serviceResult.Data, Is.EqualTo("/avatars/test.png"));
            Assert.That(account.AvatarUrl, Is.EqualTo("/avatars/test.png"));
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }

        [Test]
        public async Task RemoveAvatarAsync_AccountExists_ClearsAvatarUrlAndDeletesStoredFile()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                AvatarUrl = "/avatars/old.png",
            };

            accountRepository.AccountsById[accountId] = account;

            var serviceResult = await service.RemoveAvatarAsync(accountId);

            Assert.That(serviceResult.Success, Is.True);
            Assert.That(account.AvatarUrl, Is.Empty);
            Assert.That(avatarStorageService.DeleteCallCount, Is.EqualTo(1));
            Assert.That(avatarStorageService.LastDeletedPath, Is.EqualTo("/avatars/old.png"));
            Assert.That(accountRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(accountRepository.LastUpdatedAccount, Is.SameAs(account));
        }
    }
}
