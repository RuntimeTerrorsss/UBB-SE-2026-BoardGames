// <copyright file="AdminApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class AdminApiIntegrationTests
    {
        private readonly Guid administratorAccountId = Guid.NewGuid();
        private readonly Guid standardUserAccountId = Guid.NewGuid();

        private ApiWebApplicationFactory webApplicationFactory = null!;
        private HttpClient apiHttpClient = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.webApplicationFactory = new ApiWebApplicationFactory();

            await this.webApplicationFactory.EnsureDatabaseAsync();

            this.apiHttpClient = this.webApplicationFactory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    HandleCookies = true,
                    AllowAutoRedirect = false,
                });

            using var serviceScope = this.webApplicationFactory.Services.CreateScope();

            var applicationDbContext = serviceScope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(
                applicationDbContext,
                this.administratorAccountId,
                40,
                "admin-user",
                "admin-user@example.com",
                isAdmin: true);

            await ApiTestDataBuilder.SeedUserAsync(
                applicationDbContext,
                this.standardUserAccountId,
                41,
                "normal-user",
                "normal-user@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.apiHttpClient.Dispose();
            this.webApplicationFactory.Dispose();
        }

        private async Task AuthenticateAsAdministratorAsync()
        {
            var administratorLoginRequest = new LoginDTO
            {
                UsernameOrEmail = "admin-user",
                Password = "Password123!",
                RememberMe = false,
            };

            var administratorLoginHttpResponse =
                await this.apiHttpClient.PostAsJsonAsync(
                    "api/auth/login",
                    administratorLoginRequest);

            Assert.That(
                administratorLoginHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetAccounts_WithoutAuthentication_ReturnsUnauthorized()
        {
            var getAccountsHttpResponse =
                await this.apiHttpClient.GetAsync("api/admin/accounts");

            Assert.That(
                getAccountsHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetAccounts_WithAdministratorAuthentication_ReturnsAccounts()
        {
            await this.AuthenticateAsAdministratorAsync();

            var getAccountsHttpResponse =
                await this.apiHttpClient.GetAsync("api/admin/accounts");

            Assert.That(
                getAccountsHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            var returnedAccountProfiles =
                await getAccountsHttpResponse.Content
                    .ReadFromJsonAsync<AccountProfileDTO[]>();

            Assert.That(returnedAccountProfiles, Is.Not.Null);

            Assert.That(
                returnedAccountProfiles!
                    .Any(account => account.Id == this.administratorAccountId),
                Is.True);
        }

        [Test]
        public async Task SuspendAccount_WithValidAccount_ReturnsNoContent()
        {
            await this.AuthenticateAsAdministratorAsync();

            var suspendAccountHttpResponse =
                await this.apiHttpClient.PutAsync(
                    $"api/admin/accounts/{this.standardUserAccountId}/suspend",
                    null);

            Assert.That(
                suspendAccountHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task SuspendAccount_WithInvalidAccount_ReturnsNotFound()
        {
            await this.AuthenticateAsAdministratorAsync();

            var suspendAccountHttpResponse =
                await this.apiHttpClient.PutAsync(
                    $"api/admin/accounts/{Guid.NewGuid()}/suspend",
                    null);

            Assert.That(
                suspendAccountHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UnsuspendAccount_WithSuspendedAccount_ReturnsNoContent()
        {
            await this.AuthenticateAsAdministratorAsync();

            await this.apiHttpClient.PutAsync(
                $"api/admin/accounts/{this.standardUserAccountId}/suspend",
                null);

            var unsuspendAccountHttpResponse =
                await this.apiHttpClient.PutAsync(
                    $"api/admin/accounts/{this.standardUserAccountId}/unsuspend",
                    null);

            Assert.That(
                unsuspendAccountHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task ResetPassword_WithValidPassword_ReturnsNoContent()
        {
            await this.AuthenticateAsAdministratorAsync();

            var resetPasswordRequest = new ResetPasswordDTO
            {
                NewPassword = "NewPassword123!",
            };

            var resetPasswordHttpResponse =
                await this.apiHttpClient.PutAsJsonAsync(
                    $"api/admin/accounts/{this.standardUserAccountId}/reset-password",
                    resetPasswordRequest);

            Assert.That(
                resetPasswordHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task ResetPassword_WithWeakPassword_ReturnsBadRequest()
        {
            await this.AuthenticateAsAdministratorAsync();

            var weakPasswordResetRequest = new ResetPasswordDTO
            {
                NewPassword = "weak",
            };

            var resetPasswordHttpResponse =
                await this.apiHttpClient.PutAsJsonAsync(
                    $"api/admin/accounts/{this.standardUserAccountId}/reset-password",
                    weakPasswordResetRequest);

            Assert.That(
                resetPasswordHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UnlockAccount_WithLockedAccount_ReturnsNoContent()
        {
            await this.AuthenticateAsAdministratorAsync();

            var unlockAccountHttpResponse =
                await this.apiHttpClient.PutAsync(
                    $"api/admin/accounts/{this.standardUserAccountId}/unlock",
                    null);

            Assert.That(
                unlockAccountHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent));
        }
    }
}
