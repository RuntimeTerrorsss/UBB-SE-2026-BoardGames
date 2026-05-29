using BoardGames.Desktop.ViewModels;
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
            this.authService = new FakeClientAuthService();
            this.systemUnderTest = new RegisterViewModel(this.authService);
        }

        [Test]
        public async Task RegisterAsync_WithValidInput_SendsTrimmedRequestAndInvokesSuccessCallback()
        {
            string? successMessage = null;
            this.systemUnderTest.OnRegistrationSuccess = message => successMessage = message;
            this.systemUnderTest.DisplayName = "  Alice Example  ";
            this.systemUnderTest.Username = "  alice  ";
            this.systemUnderTest.Email = "  alice@example.com  ";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.systemUnderTest.PhoneNumber = "  0712345678  ";
            this.systemUnderTest.Country = "  Romania  ";
            this.systemUnderTest.City = "  Cluj-Napoca  ";
            this.systemUnderTest.StreetName = "  Memorandumului  ";
            this.systemUnderTest.StreetNumber = "  12A  ";

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.authService.RegisterCallCount, Is.EqualTo(1));
            Assert.That(this.authService.LastRegisterRequest, Is.Not.Null);
            Assert.That(this.authService.LastRegisterRequest!.DisplayName, Is.EqualTo("Alice Example"));
            Assert.That(this.authService.LastRegisterRequest.Username, Is.EqualTo("alice"));
            Assert.That(this.authService.LastRegisterRequest.Email, Is.EqualTo("alice@example.com"));
            Assert.That(this.authService.LastRegisterRequest.PhoneNumber, Is.EqualTo("0712345678"));
            Assert.That(this.authService.LastRegisterRequest.Country, Is.EqualTo("Romania"));
            Assert.That(this.authService.LastRegisterRequest.City, Is.EqualTo("Cluj-Napoca"));
            Assert.That(this.authService.LastRegisterRequest.StreetName, Is.EqualTo("Memorandumului"));
            Assert.That(this.authService.LastRegisterRequest.StreetNumber, Is.EqualTo("12A"));
            Assert.That(this.systemUnderTest.SuccessMessage, Is.EqualTo("Account created successfully."));
            Assert.That(successMessage, Is.EqualTo("Account created successfully. Please sign in."));
        }

        [Test]
        public async Task RegisterAsync_WithMissingDisplayName_DoesNotCallService()
        {
            this.systemUnderTest.DisplayName = string.Empty;
            this.systemUnderTest.Username = "alice";
            this.systemUnderTest.Email = "alice@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(this.systemUnderTest.DisplayNameError, Is.EqualTo("Display name is required."));
        }

        [Test]
        public async Task RegisterAsync_WithMismatchedPasswords_SetsConfirmPasswordError()
        {
            FillInValidRegistration();
            this.systemUnderTest.ConfirmPassword = "DifferentPassword!";

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(this.systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
        }

        [Test]
        public async Task RegisterAsync_WithInvalidEmail_SetsEmailError()
        {
            FillInValidRegistration();
            this.systemUnderTest.Email = "invalid-email";

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.authService.RegisterCallCount, Is.EqualTo(0));
            Assert.That(this.systemUnderTest.EmailError, Is.EqualTo("A valid email is required."));
        }

        [Test]
        public async Task RegisterAsync_WhenServiceFails_ShowsReturnedError()
        {
            FillInValidRegistration();
            this.authService.RegisterResult = ServiceResult.Fail("Username is already taken.");

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Username is already taken."));
            Assert.That(this.systemUnderTest.SuccessMessage, Is.EqualTo(string.Empty));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
        }

        [Test]
        public void GoToLogin_WhenExecuted_InvokesNavigationCallback()
        {
            bool navigateToLoginWasCalled = false;
            this.systemUnderTest.OnNavigateToLogin = () => navigateToLoginWasCalled = true;

            this.systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.That(navigateToLoginWasCalled, Is.True);
        }

        private void FillInValidRegistration()
        {
            this.systemUnderTest.DisplayName = "Alice Example";
            this.systemUnderTest.Username = "alice";
            this.systemUnderTest.Email = "alice@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.systemUnderTest.PhoneNumber = "0712345678";
            this.systemUnderTest.Country = "Romania";
            this.systemUnderTest.City = "Cluj-Napoca";
            this.systemUnderTest.StreetName = "Memorandumului";
            this.systemUnderTest.StreetNumber = "12A";
        }
    }
}
