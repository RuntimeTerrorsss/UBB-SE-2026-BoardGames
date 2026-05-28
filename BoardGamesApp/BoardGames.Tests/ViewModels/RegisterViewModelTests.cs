// <copyright file="RegisterViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
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
        public async Task RegisterAsync_WithValidInput_SendsTrimmedRequestAndInvokesSuccessCallback()
        {
            string? successMessage = null;
            systemUnderTest.OnRegistrationSuccess = message => successMessage = message;
            systemUnderTest.DisplayName = "  Alice Example  ";
            systemUnderTest.Username = "  alice  ";
            systemUnderTest.Email = "  alice@example.com  ";
            systemUnderTest.Password = "Password123!";
            systemUnderTest.ConfirmPassword = "Password123!";
            systemUnderTest.PhoneNumber = "  0712345678  ";
            systemUnderTest.Country = "  Romania  ";
            systemUnderTest.City = "  Cluj-Napoca  ";
            systemUnderTest.StreetName = "  Memorandumului  ";
            systemUnderTest.StreetNumber = "  12A  ";

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(authService.RegisterCallCount, Is.EqualTo(1));
            Assert.That(authService.LastRegisterRequest, Is.Not.Null);
            Assert.That(authService.LastRegisterRequest!.DisplayName, Is.EqualTo("Alice Example"));
            Assert.That(authService.LastRegisterRequest.Username, Is.EqualTo("alice"));
            Assert.That(authService.LastRegisterRequest.Email, Is.EqualTo("alice@example.com"));
            Assert.That(authService.LastRegisterRequest.PhoneNumber, Is.EqualTo("0712345678"));
            Assert.That(authService.LastRegisterRequest.Country, Is.EqualTo("Romania"));
            Assert.That(authService.LastRegisterRequest.City, Is.EqualTo("Cluj-Napoca"));
            Assert.That(authService.LastRegisterRequest.StreetName, Is.EqualTo("Memorandumului"));
            Assert.That(authService.LastRegisterRequest.StreetNumber, Is.EqualTo("12A"));
            Assert.That(systemUnderTest.SuccessMessage, Is.EqualTo("Account created successfully."));
            Assert.That(successMessage, Is.EqualTo("Account created successfully. Please sign in."));
        }

        [Test]
        public async Task RegisterAsync_WithMissingDisplayName_DoesNotCallService()
        {
            systemUnderTest.DisplayName = string.Empty;
            systemUnderTest.Username = "alice";
            systemUnderTest.Email = "alice@example.com";
            systemUnderTest.Password = "Password123!";
            systemUnderTest.ConfirmPassword = "Password123!";

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(systemUnderTest.DisplayNameError, Is.EqualTo("Display name is required."));
        }

        [Test]
        public async Task RegisterAsync_WithMismatchedPasswords_SetsConfirmPasswordError()
        {
            FillInValidRegistration();
            systemUnderTest.ConfirmPassword = "DifferentPassword!";

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
        }

        [Test]
        public async Task RegisterAsync_WithInvalidEmail_SetsEmailError()
        {
            FillInValidRegistration();
            systemUnderTest.Email = "invalid-email";

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(systemUnderTest.EmailError, Is.EqualTo("A valid email is required."));
        }

        [Test]
        public async Task RegisterAsync_WhenServiceFails_ShowsReturnedError()
        {
            FillInValidRegistration();
            authService.RegisterResult = ServiceResult.Fail("Username is already taken.");

            await systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(systemUnderTest.ErrorMessage, Is.EqualTo("Username is already taken."));
            Assert.That(systemUnderTest.SuccessMessage, Is.EqualTo(string.Empty));
            Assert.That(systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void GoToLogin_WhenExecuted_InvokesNavigationCallback()
        {
            bool navigateToLoginWasCalled = false;
            systemUnderTest.OnNavigateToLogin = () => navigateToLoginWasCalled = true;

            systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.That(navigateToLoginWasCalled, Is.True);
        }

        private void FillInValidRegistration()
        {
            systemUnderTest.DisplayName = "Alice Example";
            systemUnderTest.Username = "alice";
            systemUnderTest.Email = "alice@example.com";
            systemUnderTest.Password = "Password123!";
            systemUnderTest.ConfirmPassword = "Password123!";
            systemUnderTest.PhoneNumber = "0712345678";
            systemUnderTest.Country = "Romania";
            systemUnderTest.City = "Cluj-Napoca";
            systemUnderTest.StreetName = "Memorandumului";
            systemUnderTest.StreetNumber = "12A";
        }
    }
}
