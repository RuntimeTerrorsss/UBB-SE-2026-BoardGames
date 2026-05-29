using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class AdminViewModelTests
    {
        private FakeClientAdminService adminService = null!;
        private Mock<IDesktopAuthorizationService> mockAuthService = null!;
        private AdminViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.adminService = new FakeClientAdminService();
            this.mockAuthService = new Mock<IDesktopAuthorizationService>();

            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(true);
            this.mockAuthService.Setup(a => a.IsLoggedIn).Returns(true);

            this.systemUnderTest = new AdminViewModel(this.adminService, this.mockAuthService.Object);
        }

        [Test]
        public async Task LoadAccountsAsync_ShouldSetErrorMessage_WhenUserIsNotAdmin()
        {
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(false);

            await this.systemUnderTest.LoadAccountsAsync();

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Unauthorized access. Administrator role is required."));
            Assert.That(this.systemUnderTest.PagedItems, Is.Empty);
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public async Task LoadAccountsAsync_ShouldPopulatePagedItems_WhenServiceReturnsData()
        {
            var accounts = new List<AccountProfileDTO>
            {
                new AccountProfileDTO { Username = "user1", DisplayName = "User One" },
                new AccountProfileDTO { Username = "user2", DisplayName = "User Two" },
            };

            this.adminService.AccountsResult = ServiceResult<IEnumerable<AccountProfileDTO>>.Ok(accounts);

            await this.systemUnderTest.LoadAccountsAsync();

            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo("user1"));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
            Assert.That(this.systemUnderTest.ErrorMessage, Is.Empty);
        }

        [Test]
        public void SelectedAccount_ShouldEnableCommands_WhenAccountIsChanged()
        {
            var selectedAccount = new AccountProfileDTO { Username = "target" };

            this.systemUnderTest.SelectedAccount = selectedAccount;

            Assert.That(this.systemUnderTest.SuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnsuspendAccountCommand.CanExecute(null), Is.True);
            Assert.That(this.systemUnderTest.UnlockAccountCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void SuspendAccountAsync_ShouldCallServiceAndReload_WhenAdminAndAccountSelected()
        {
            int pageSize = PagedViewModel<AccountProfileDTO>.PageSize;
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = accountId, Username = "victim" };

            this.adminService.SuspendResult = ServiceResult<bool>.Ok(true);
            this.adminService.AccountsResult = ServiceResult<IEnumerable<AccountProfileDTO>>.Ok(new List<AccountProfileDTO>());

            this.systemUnderTest.SuspendAccountCommand.Execute(null);

            Assert.That(this.adminService.SuspendCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.GetAllAccountsCallCount, Is.EqualTo(1));
            Assert.That(this.adminService.LastPage, Is.EqualTo(1));
            Assert.That(this.adminService.LastPageSize, Is.EqualTo(pageSize));
        }

        [Test]
        public void SuspendAccountAsync_ShouldNotCallService_WhenUserIsNotAdmin()
        {
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(false);
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = Guid.NewGuid() };

            this.systemUnderTest.SuspendAccountCommand.Execute(null);

            Assert.That(this.adminService.SuspendCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task NextPageCommand_ShouldAdvanceCurrentPage_WhenMultiplePagesExist()
        {
            int pageSize = PagedViewModel<AccountProfileDTO>.PageSize;
            var accounts = new List<AccountProfileDTO>();
            for (int accountIndex = 1; accountIndex <= pageSize + 1; accountIndex++)
            {
                accounts.Add(new AccountProfileDTO { Username = $"user{accountIndex}" });
            }

            this.adminService.AccountsResult = ServiceResult<IEnumerable<AccountProfileDTO>>.Ok(accounts);

            await this.systemUnderTest.LoadAccountsAsync();
            this.systemUnderTest.NextPageCommand.Execute(null);

            Assert.That(this.systemUnderTest.CurrentPage, Is.EqualTo(2));
            Assert.That(this.systemUnderTest.PagedItems.Count, Is.EqualTo(1));
            Assert.That(this.systemUnderTest.PagedItems[0].Username, Is.EqualTo($"user{pageSize + 1}"));
        }

        [Test]
        public void UnlockAccountAsync_ShouldSetSuccessMessage_WhenCallIsSuccessful()
        {
            Guid accountId = Guid.NewGuid();
            this.systemUnderTest.SelectedAccount = new AccountProfileDTO { Id = accountId };

            this.adminService.UnlockResult = ServiceResult<bool>.Ok(true);

            this.systemUnderTest.UnlockAccountCommand.Execute(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Account unlocked."));
        }

        [Test]
        public async Task ResetPasswordWithValueAsync_ShouldSetSuccessMessage_WhenPasswordIsValid()
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