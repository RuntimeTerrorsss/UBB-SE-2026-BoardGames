using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class RegisterViewModelTests
    {
        private FakeClientAuthService authService = null!;
        private RegisterViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            authService = new FakeClientAuthService();
            systemUnderTest = new RegisterViewModel(authService);
        }

        [Test]
        public async Task RegisterAsync_SuccessfulRegistration_InvokesSuccessCallback()
        {
            bool registrationSuccessCallbackWasCalled = false;
            systemUnderTest.OnRegistrationSuccess = () => registrationSuccessCallbackWasCalled = true;
            systemUnderTest.Username = "newuser";
            systemUnderTest.Password = "Password123!";
            systemUnderTest.ConfirmPassword = "Password123!";

            authService.RegisterResult = ServiceResult<bool>.Ok(true);

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(registrationSuccessCallbackWasCalled, Is.True);
            Assert.That(authService.RegisterCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RegisterAsync_FieldValidationError_MapsErrorsToCorrectProperties()
        {
            string validationError = "Username|Username already exists;Password|Password is too short";

            authService.RegisterResult = ServiceResult<bool>.Fail(validationError);

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.UsernameError, Is.EqualTo("Username already exists"));
            Assert.That(systemUnderTest.PasswordError, Is.EqualTo("Password is too short"));
            Assert.That(systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public async Task RegisterAsync_GeneralError_SetsGeneralErrorMessage()
        {
            string generalError = "Server connection lost";

            authService.RegisterResult = ServiceResult<bool>.Fail(generalError);

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo(generalError));
            Assert.That(systemUnderTest.EmailError, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GoToLogin_WhenExecuted_InvokesNavigateBackRequest()
        {
            bool navigateBackWasCalled = false;
            systemUnderTest.OnNavigateBackRequest = () => navigateBackWasCalled = true;

            systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.That(navigateBackWasCalled, Is.True);
        }

        [Test]
        public async Task RegisterAsync_ClearsOldErrorsBeforeNewAttempt()
        {
            systemUnderTest.UsernameError = "Old error";

            authService.RegisterResult = ServiceResult<bool>.Ok(true);

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.UsernameError, Is.EqualTo(string.Empty));
        }
    }
}
