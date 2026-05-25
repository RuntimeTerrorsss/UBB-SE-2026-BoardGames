using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class LoginViewModelTests
    {
        private FakeClientAuthService authService = null!;
        private LoginViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            authService = new FakeClientAuthService();
            systemUnderTest = new LoginViewModel(authService);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_InvokesSuccessCallbackWithRole()
        {
            string capturedRole = string.Empty;
            systemUnderTest.OnLoginSuccess = role => capturedRole = role;
            systemUnderTest.UsernameOrEmail = "admin";
            systemUnderTest.Password = "Password123!";

            var profile = new AccountProfileDataTransferObject
            {
                Username = "admin",
                Role = new RoleDataTransferObject { Name = "Administrator" },
            };

            authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(profile);

            await systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Administrator"));
            Assert.That(authService.LoginCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoginAsync_EmptyFields_SetsLocalErrorMessageWithoutCallingService()
        {
            systemUnderTest.UsernameOrEmail = string.Empty;
            systemUnderTest.Password = string.Empty;

            await systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo("Please enter both username/email and password."));
            Assert.That(authService.LoginCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task LoginAsync_ServiceReturnsError_SetsErrorMessage()
        {
            systemUnderTest.UsernameOrEmail = "user";
            systemUnderTest.Password = "wrongpass";

            string serviceError = "Invalid username or password.";
            authService.LoginResult =
                ServiceResult<AccountProfileDataTransferObject>.Fail(serviceError);

            await systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo(serviceError));
            Assert.That(systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void NavigateToRegister_WhenExecuted_InvokesCallback()
        {
            bool navigationWasCalled = false;
            systemUnderTest.OnNavigateToRegister = () => navigationWasCalled = true;

            systemUnderTest.NavigateToRegisterCommand.Execute(null);

            Assert.That(navigationWasCalled, Is.True);
        }

        [Test]
        public async Task LoginAsync_NullRole_DefaultsToStandardUser()
        {
            string capturedRole = string.Empty;
            systemUnderTest.OnLoginSuccess = role => capturedRole = role;
            systemUnderTest.UsernameOrEmail = "user";
            systemUnderTest.Password = "pass";

            var profile = new AccountProfileDataTransferObject
            {
                Username = "user",
                Role = null!,
            };

            authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(profile);

            await systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(capturedRole, Is.EqualTo("Standard User"));
        }
    }
}
