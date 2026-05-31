using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

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
            adminService = new FakeClientAdminService();
            systemUnderTest = new AdminViewModel(adminService);
        }

        [Test]
        public async Task LoadAccountsAsync_ServiceReturnsData_PopulatesPagedItems()
        {
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            var accounts = new List<AccountProfileDataTransferObject>
            {
                new AccountProfileDataTransferObject { Username = "user1", DisplayName = "User One" },
                new AccountProfileDataTransferObject { Username = "user2", DisplayName = "User Two" },
            };

            adminService.AccountsResult =
                ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts);

            await systemUnderTest.LoadAccountsAsync();

            Assert.That(systemUnderTest.PagedItems.Count, Is.EqualTo(2));
            Assert.That(systemUnderTest.PagedItems[0].Username, Is.EqualTo("user1"));
            Assert.That(systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void SelectedAccount_WhenChanged_EnablesCommands()
        {
            var selectedAccount = new AccountProfileDataTransferObject { Username = "target" };

            systemUnderTest.SelectedAccount = selectedAccount;

            Assert.That(systemUnderTest.SuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(systemUnderTest.UnsuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(systemUnderTest.UnlockAccountCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task SuspendAccountAsync_SelectedAccount_CallsServiceAndReloadsAccounts()
        {
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            Guid accountId = Guid.NewGuid();
            systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId, Username = "victim" };

            adminService.SuspendResult = ServiceResult<bool>.Ok(true);

            await systemUnderTest.SuspendAccountCommand.ExecuteAsync(null);

            Assert.That(adminService.SuspendCallCount, Is.EqualTo(1));
            Assert.That(adminService.GetAllAccountsCallCount, Is.EqualTo(1));
            Assert.That(adminService.LastPage, Is.EqualTo(1));
            Assert.That(adminService.LastPageSize, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task NextPageCommand_WhenMultiplePagesExist_AdvancesCurrentPage()
        {
            int pageSize = PagedViewModel<AccountProfileDataTransferObject>.PageSize;
            var accounts = new List<AccountProfileDataTransferObject>();
            for (int accountIndex = 1; accountIndex <= pageSize + 1; accountIndex++)
            {
                accounts.Add(new AccountProfileDataTransferObject { Username = $"user{accountIndex}" });
            }

            adminService.AccountsResult =
                ServiceResult<List<AccountProfileDataTransferObject>>.Ok(accounts);

            await systemUnderTest.LoadAccountsAsync();
            systemUnderTest.NextPageCommand.Execute(null);

            Assert.That(systemUnderTest.CurrentPage, Is.EqualTo(2));
            Assert.That(systemUnderTest.PagedItems.Count, Is.EqualTo(1));
            Assert.That(systemUnderTest.PagedItems[0].Username, Is.EqualTo($"user{pageSize + 1}"));
        }

        [Test]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };

            adminService.UnlockResult = ServiceResult<bool>.Ok(true);

            await systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo("Account unlocked."));
        }

        [Test]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            Guid accountId = Guid.NewGuid();
            systemUnderTest.SelectedAccount = new AccountProfileDataTransferObject { Id = accountId };
            string newPassword = "NewSecurePass123!";

            adminService.ResetPasswordResult = ServiceResult<bool>.Ok(true);

            await systemUnderTest.ResetPasswordWithValueAsync(newPassword);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo("Password reset successful."));
        }
    }
}
