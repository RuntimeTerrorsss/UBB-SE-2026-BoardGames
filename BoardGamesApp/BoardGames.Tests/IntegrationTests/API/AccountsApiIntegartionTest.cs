// <copyright file="AccountsApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Shared.DTO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class AccountsApiIntegrationTests
    {
        private readonly Guid accountId = Guid.NewGuid();
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();
            this.client = this.factory.CreateClient();

            using var scope = this.factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ApiTestDataBuilder.SeedUserAsync(dbContext, this.accountId, 30, "profile-user", "profile-user@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task GetProfile_ReturnsSeededAccount()
        {
            var response = await this.client.GetAsync($"api/accounts/{this.accountId}");
            response.EnsureSuccessStatusCode();

            var profile = await response.Content.ReadFromJsonAsync<AccountProfileDTO>();
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile!.Id, Is.EqualTo(this.accountId));
            Assert.That(profile.Username, Is.EqualTo("profile-user"));
        }

        [Test]
        public async Task GetProfile_WithNonExistentId_ReturnsNotFound()
        {
            var response = await this.client.GetAsync($"api/accounts/{Guid.NewGuid()}");
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UpdateProfile_ThenGetProfile_ReturnsUpdatedFields()
        {
            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                DisplayName = "Updated Name",
                Email = "profile-user@example.com",
                PhoneNumber = "+40123123123",
                Country = "Romania",
                City = "Cluj",
                StreetName = "Main",
                StreetNumber = "10",
            };

            var updateResponse = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            updateResponse.EnsureSuccessStatusCode();

            var getResponse = await this.client.GetAsync($"api/accounts/{this.accountId}");
            getResponse.EnsureSuccessStatusCode();
            var profile = await getResponse.Content.ReadFromJsonAsync<AccountProfileDTO>();

            Assert.That(profile!.DisplayName, Is.EqualTo("Updated Name"));
            Assert.That(profile.PhoneNumber, Is.EqualTo("+40123123123"));
        }

        [Test]
        public async Task UpdateProfile_WithNonExistentId_ReturnsNotFound()
        {
            var update = new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                DisplayName = "Valid Name",
                Email = "someone@example.com",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{Guid.NewGuid()}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UpdateProfile_WithTooShortDisplayName_ReturnsValidationError()
        {
            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                Email = "profile-user@example.com",
                DisplayName = "A",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProfile_WithTooLongDisplayName_ReturnsValidationError()
        {
            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                Email = "profile-user@example.com",
                DisplayName = new string('A', 51),
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProfile_WithInvalidPhoneNumber_ReturnsValidationError()
        {
            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                Email = "profile-user@example.com",
                DisplayName = "Valid Name",
                PhoneNumber = "abc-not-a-phone",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProfile_WithTooLongStreetNumber_ReturnsValidationError()
        {
            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                Email = "profile-user@example.com",
                DisplayName = "Valid Name",
                StreetNumber = new string('1', 11),
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProfile_WithDuplicateEmail_ReturnsConflict()
        {
            var otherAccountId = Guid.NewGuid();
            using var scope = this.factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ApiTestDataBuilder.SeedUserAsync(dbContext, otherAccountId, 25, "other-user", "other-user@example.com");

            var update = new AccountProfileDTO
            {
                Id = this.accountId,
                Username = "profile-user",
                DisplayName = "Valid Name",
                Email = "other-user@example.com",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}", update);
            Assert.That((int)response.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task ChangePassword_ReturnsNoContent()
        {
            var changePassword = new ChangePasswordDTO
            {
                CurrentPassword = "Password123!",
                NewPassword = "Password1234!",
                ConfirmPassword = "Password1234!",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}/password", changePassword);
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ChangePassword_WithWrongCurrentPassword_ReturnsUnauthorized()
        {
            var changePassword = new ChangePasswordDTO
            {
                CurrentPassword = "WrongPassword!",
                NewPassword = "Password1234!",
                ConfirmPassword = "Password1234!",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}/password", changePassword);
            Assert.That((int)response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ChangePassword_WithWeakNewPassword_ReturnsValidationError()
        {
            var changePassword = new ChangePasswordDTO
            {
                CurrentPassword = "Password123!",
                NewPassword = "weak",
                ConfirmPassword = "weak",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{this.accountId}/password", changePassword);
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ChangePassword_WithNonExistentId_ReturnsNotFound()
        {
            var changePassword = new ChangePasswordDTO
            {
                CurrentPassword = "Password123!",
                NewPassword = "Password1234!",
                ConfirmPassword = "Password1234!",
            };

            var response = await this.client.PutAsJsonAsync($"api/accounts/{Guid.NewGuid()}/password", changePassword);
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }
    }
}
