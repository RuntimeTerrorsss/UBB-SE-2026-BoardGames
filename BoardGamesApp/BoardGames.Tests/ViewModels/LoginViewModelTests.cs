// <copyright file="LoginViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
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
        public async Task Login_ValidCredentials_PopulatesSessionAndCallsSuccessHook()
        {
            bool callbackWasCalled = false;
            this.systemUnderTest.OnLoginSuccess = () => callbackWasCalled = true;
            this.systemUnderTest.UsernameOrEmail = "admin";
            this.systemUnderTest.Password = "Password123!";

            var accountId = Guid.NewGuid();
            var profile = new AccountProfileDTO
            {
                Id = accountId,
                PamUserId = 42,
                Username = "admin",
                DisplayName = "Admin User",
                Email = "admin@example.com",
                Role = new RoleDTO { Name = "Administrator" },
                AvatarUrl = "/avatars/admin.png",
                IsSuspended = true,
                IsLocked = true,
                PhoneNumber = "0712345678",
                Country = "Romania",
                City = "Cluj-Napoca",
                StreetName = "Memorandumului",
                StreetNumber = "10",
            };

            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(profile);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(callbackWasCalled, Is.True);
                Assert.That(this.authService.LoginCallCount, Is.EqualTo(1));
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(1));
                Assert.That(this.sessionContext.IsLoggedIn, Is.True);
                Assert.That(this.sessionContext.AccountId, Is.EqualTo(accountId));
                Assert.That(this.sessionContext.PamUserId, Is.EqualTo(42));
                Assert.That(this.sessionContext.Role, Is.EqualTo(AppRoles.Administrator));
                Assert.That(this.sessionContext.DisplayName, Is.EqualTo("Admin User"));
                Assert.That(this.sessionContext.City, Is.EqualTo("Cluj-Napoca"));
            }
        }

        [Test]
        public async Task Login_UsernameContainsWhitespace_TrimsUsernameAndKeepsRememberMe()
        {
            this.systemUnderTest.UsernameOrEmail = "  admin@example.com  ";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.RememberMe = true;

            this.authService.LoginResult =
                ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO { Id = Guid.NewGuid() });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.authService.LastLoginRequest, Is.Not.Null);
                Assert.That(this.authService.LastLoginRequest!.UsernameOrEmail, Is.EqualTo("admin@example.com"));
                Assert.That(this.authService.LastLoginRequest.RememberMe, Is.True);
            }
        }

        [Test]
        public async Task Login_ServiceReturnsError_ShowsErrorAndLeavesSessionEmpty()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "wrongpass";

            string serviceError = "Invalid username or password.";
            this.authService.LoginResult =
                ServiceResult<AccountProfileDTO>.Fail(serviceError);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(serviceError));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
                Assert.That(this.sessionContext.IsLoggedIn, Is.False);
            }
        }

        [Test]
        public async Task Login_ProfileHasNoRole_UsesDefaultRole()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "pass";

            var profile = new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = "user",
                Role = null,
            };

            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(profile);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            Assert.That(this.sessionContext.Role, Is.EqualTo(AppRoles.StandardUser));
        }

        [Test]
        public async Task Login_ServiceReturnsErrorWithoutMessage_UsesGenericErrorMessage()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "Password123!";
            this.authService.LoginResult = new ServiceResult<AccountProfileDTO> { Success = false };

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Login failed."));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task Login_ServiceReturnsSuccessWithoutProfile_DoesNotPopulateSession()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "Password123!";
            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(null!);

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Login failed."));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
                Assert.That(this.sessionContext.IsLoggedIn, Is.False);
            }
        }

        [Test]
        public async Task Login_SuccessCallbackIsMissing_PopulatesSessionWithoutThrowing()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "Password123!";
            this.authService.LoginResult = ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = "user",
            });

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.sessionContext.IsLoggedIn, Is.True);
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(string.Empty));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
            }
        }

        [Test]
        public void Login_ServiceThrowsException_ResetsLoadingStateAndPropagatesException()
        {
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "Password123!";
            this.authService.LoginException = new InvalidOperationException("Login exploded.");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(new Func<Task>(async () =>
                await this.systemUnderTest.LoginCommand.ExecuteAsync(null)));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception!.Message, Is.EqualTo("Login exploded."));
                Assert.That(this.systemUnderTest.IsLoading, Is.False);
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task Login_EmptyFields_ShowsLocalValidationAndSkipsServiceCall()
        {
            this.systemUnderTest.UsernameOrEmail = string.Empty;
            this.systemUnderTest.Password = string.Empty;

            await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Please enter both username/email and password."));
                Assert.That(this.authService.LoginCallCount, Is.EqualTo(0));
                Assert.That(this.sessionContext.PopulateCallCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void NavigateToRegister_CallbackIsSet_CallsNavigationHook()
        {
            bool navigationWasCalled = false;
            this.systemUnderTest.OnNavigateToRegister = () => navigationWasCalled = true;

            this.systemUnderTest.NavigateToRegisterCommand.Execute(null);

            Assert.That(navigationWasCalled, Is.True);
        }

        [Test]
        public void NavigateToRegister_CallbackIsMissing_DoesNotThrow()
        {
            Assert.That(new Action(() => this.systemUnderTest.NavigateToRegisterCommand.Execute(null)), Throws.Nothing);
        }
    }
}
