// <copyright file="LoginViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class LoginViewModelTests
    {
        private FakeClientAuthService authService = null!;
        private FakeSessionContext sessionContext = null!;
        private LoginViewModel systemUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            this.authService = new FakeClientAuthService();
            this.sessionContext = new FakeSessionContext();
            this.systemUnderTest = new LoginViewModel(this.authService, this.sessionContext);
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_PopulatesSessionAndInvokesSuccessCallback()
        {
            bool successCallbackWasCalled = false;
            this.systemUnderTest.OnLoginSuccess = () => successCallbackWasCalled = true;
            this.systemUnderTest.UsernameOrEmail = "admin";
            this.systemUnderTest.Password = "Password123!";

            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                DisplayName = "Administrator",
                Role = new RoleDTO { Name = AppRoles.Administrator },
            });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(successCallbackWasCalled, Is.True);
            Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(1));
            Assert.That(this.sessionContext.Username, Is.EqualTo("admin"));
            Assert.That(this.authService.LoginCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoginAsync_WithBlankUsernameAndPassword_SetsValidationErrorWithoutCallingService()
        {
            this.systemUnderTest.UsernameOrEmail = string.Empty;
            this.systemUnderTest.Password = string.Empty;

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Please enter both username/email and password."));
            Assert.That(this.authService.LoginCallCount, Is.EqualTo(0));
            Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task LoginAsync_WhenAuthServiceFails_ShowsReturnedErrorAndStopsLoading()
        {
            this.systemUnderTest.UsernameOrEmail = "player";
            this.systemUnderTest.Password = "bad-password";
            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Fail("Invalid username or password.");

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Invalid username or password."));
            Assert.That(this.systemUnderTest.IsLoading, Is.False);
            Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
        }

        [Test]
        public void NavigateToRegister_WhenExecuted_InvokesNavigationCallback()
        {
            bool navigateToRegisterWasCalled = false;
            this.systemUnderTest.OnNavigateToRegister = () => navigateToRegisterWasCalled = true;

            this.systemUnderTest.NavigateToRegisterCommand.Execute(null);

            Assert.That(navigateToRegisterWasCalled, Is.True);
        }

        [Test]
        public async Task LoginAsync_AfterPreviousMessages_ClearsOldErrorAndInfoOnSuccess()
        {
            this.systemUnderTest.ErrorMessage = "Old error";
            this.systemUnderTest.InfoMessage = "Old info";
            this.systemUnderTest.UsernameOrEmail = "member";
            this.systemUnderTest.Password = "Password123!";
            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = "member",
            });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(string.Empty));
            Assert.That(this.systemUnderTest.InfoMessage, Is.EqualTo(string.Empty));
        }
    }
}
