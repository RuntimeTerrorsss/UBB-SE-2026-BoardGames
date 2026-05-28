// <copyright file="AdminViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using NUnit.Framework;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class AdminViewModelTests
    {
        private FakeClientAdminService adminService = null!;
        private AdminViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.adminService = new FakeClientAdminService();
            this.systemUnderTest = new AdminViewModel(this.adminService);
        }

        [Test]
        public async Task LoadAccountsAsync_ServiceReturnsData_PopulatesPagedItems()
        {
            int pageSize = PagedViewModel<AccountProfileDTO>.PageSize;
            var accounts = new List<AccountProfileDTO>
            {
                new AccountProfileDTO { Username = "user1", DisplayName = "User One" },
                new AccountProfileDTO { Username = "user2", DisplayName = "User Two" },
            };

            this.adminService.AccountsResult =
                ServiceResult<List<AccountProfileDTO>>.Ok(accounts);

            await this.systemUnderTest.LoadAccountsAsync();

            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo("user1"));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void SelectedAccount_WhenChanged_EnablesCommands()
        {
            var selectedAccount = new AccountProfileDTO { Username = "target" };

            this.systemUnderTest.SelectedAccount = selectedAccount;

            Assert.That(this.systemUnderTest.SuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnsuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnlockAccountCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task SuspendAccountAsync_SelectedAccount_CallsServiceAndReloadsAccounts()
        {
            int pageSize = PagedViewModel<AccountProfileDTO>.PageSize;
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = accountId, Username = "victim" };

            this.adminService.SuspendResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.SuspendAccountCommand.ExecuteAsync(null);

            Assert.That(this.adminService.SuspendCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.GetAllAccountsCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.LastPage, Is.EqualTo(1));
            Assert.That(this.adminService.LastPageSize, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task NextPageCommand_WhenMultiplePagesExist_AdvancesCurrentPage()
        {
            int pageSize = PagedViewModel<AccountProfileDTO>.PageSize;
            var accounts = new List<AccountProfileDTO>();
            for (int accountIndex = 1; accountIndex <= pageSize + 1; accountIndex++)
            {
                accounts.Add(new AccountProfileDTO { Username = $"user{accountIndex}" });
            }

            this.adminService.AccountsResult =
                ServiceResult<List<AccountProfileDTO>>.Ok(accounts);

            await this.systemUnderTest.LoadAccountsAsync();
            this.systemUnderTest.NextPageCommand.Execute(null);

            Assert.That(this.systemUnderTest.CurrentPage, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(1));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo($"user{pageSize + 1}"));
        }

        [Test]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = accountId };

            this.adminService.UnlockResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Account unlocked."));
        }

        [Test]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = accountId };
            string newPassword = "NewSecurePass123!";

            this.adminService.ResetPasswordResult = ServiceResult<bool>.Ok(true);

            await this.systemUnderTest.ResetPasswordWithValueAsync(newPassword);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Password reset successful."));
        }
    }
}
