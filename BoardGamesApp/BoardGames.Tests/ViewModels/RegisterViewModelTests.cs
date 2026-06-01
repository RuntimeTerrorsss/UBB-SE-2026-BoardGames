// <copyright file="RegisterViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BoardGames.Desktop.ViewModels;
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
        public async Task Register_ValidInput_SendsTrimmedDataAndShowsSuccess()
        {
            string? successMessage = null;
            this.systemUnderTest.OnRegistrationSuccess = message => successMessage = message;
            this.systemUnderTest.DisplayName = "New User";
            this.systemUnderTest.Username = "  newuser  ";
            this.systemUnderTest.Email = "  newuser@example.com  ";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.systemUnderTest.PhoneNumber = "0712345678";
            this.systemUnderTest.Country = "  Romania  ";
            this.systemUnderTest.City = "  Cluj-Napoca  ";
            this.systemUnderTest.StreetName = "  Memorandumului  ";
            this.systemUnderTest.StreetNumber = "  10  ";

            this.authService.RegisterResult = ServiceResult.Ok();

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(successMessage, Is.EqualTo("Account created successfully. Please sign in."));
                Assert.That(this.systemUnderTest.SuccessMessage, Is.EqualTo("Account created successfully."));
                Assert.That(this.authService.RegisterCallCount, Is.EqualTo(1));
                Assert.That(this.authService.LastRegisterRequest, Is.Not.Null);
                Assert.That(this.authService.LastRegisterRequest!.DisplayName, Is.EqualTo("New User"));
                Assert.That(this.authService.LastRegisterRequest.Username, Is.EqualTo("newuser"));
                Assert.That(this.authService.LastRegisterRequest.Email, Is.EqualTo("newuser@example.com"));
                Assert.That(this.authService.LastRegisterRequest.Country, Is.EqualTo("Romania"));
                Assert.That(this.authService.LastRegisterRequest.City, Is.EqualTo("Cluj-Napoca"));
                Assert.That(this.authService.LastRegisterRequest.StreetName, Is.EqualTo("Memorandumului"));
                Assert.That(this.authService.LastRegisterRequest.StreetNumber, Is.EqualTo("10"));
            }
        }

        [Test]
        public async Task Register_InvalidFields_ShowsValidationErrorsLocally()
        {
            this.systemUnderTest.DisplayName = string.Empty;
            this.systemUnderTest.Username = string.Empty;
            this.systemUnderTest.Email = "invalid-email";
            this.systemUnderTest.Password = "123";
            this.systemUnderTest.ConfirmPassword = "456";

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.DisplayNameError, Is.EqualTo("Display name is required."));
                Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo("Username is required."));
                Assert.That(this.systemUnderTest.EmailError, Is.EqualTo("A valid email is required."));
                Assert.That(this.systemUnderTest.PasswordError, Is.EqualTo("Password must be at least 6 characters."));
                Assert.That(this.systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
                Assert.That(this.authService.RegisterCallCount, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task Register_ServiceReturnsError_ShowsServiceError()
        {
            this.systemUnderTest.DisplayName = "New User";
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Email = "newuser@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            string generalError = "Server connection lost";

            this.authService.RegisterResult = ServiceResult.Fail(generalError);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(generalError));
            Assert.That(this.systemUnderTest.EmailError, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task Register_ServiceReturnsErrorWithoutMessage_UsesGenericErrorMessage()
        {
            this.systemUnderTest.DisplayName = "New User";
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Email = "newuser@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.authService.RegisterResult = new ServiceResult { Success = false };

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Registration failed."));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
            }
        }

        [Test]
        public void Register_ServiceThrowsException_ResetsLoadingStateAndPropagatesException()
        {
            this.systemUnderTest.DisplayName = "New User";
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Email = "newuser@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";
            this.authService.RegisterException = new InvalidOperationException("Registration exploded.");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(new Func<Task>(async () =>
                await this.systemUnderTest.RegisterCommand.ExecuteAsync(null)));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception!.Message, Is.EqualTo("Registration exploded."));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
            }
        }

        [Test]
        public void GoToLogin_CallbackIsSet_CallsNavigationHook()
        {
            bool navigateToLoginWasCalled = false;
            this.systemUnderTest.OnNavigateToLogin = () => navigateToLoginWasCalled = true;

            this.systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.That(navigateToLoginWasCalled, Is.True);
        }

        [Test]
        public async Task Register_ValidInput_ClearsOldValidationStateBeforeRetry()
        {
            this.systemUnderTest.UsernameError = "Old error";
            this.systemUnderTest.DisplayName = "New User";
            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Email = "newuser@example.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            this.authService.RegisterResult = ServiceResult.Ok();

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.UsernameError, Is.EqualTo(string.Empty));
        }
    }
}
