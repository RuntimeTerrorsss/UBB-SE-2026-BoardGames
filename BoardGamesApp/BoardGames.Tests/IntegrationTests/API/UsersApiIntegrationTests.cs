// <copyright file="UsersApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
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
    public sealed class UsersApiIntegrationTests
    {
        private readonly Guid excludedMainUserAccountId = Guid.NewGuid();
        private readonly Guid secondaryTestUserAccountId = Guid.NewGuid();
        private readonly Guid tertiaryTestUserAccountId = Guid.NewGuid();

        private ApiWebApplicationFactory apiTestApplicationFactory = null!;
        private HttpClient apiHttpClient = null!;

        [SetUp]
        public async Task SetUp_TestDatabaseAndHttpClient()
        {
            this.apiTestApplicationFactory = new ApiWebApplicationFactory();
            await this.apiTestApplicationFactory.EnsureDatabaseAsync();
            this.apiHttpClient = this.apiTestApplicationFactory.CreateClient();

            using var serviceScope = this.apiTestApplicationFactory.Services.CreateScope();
            var databaseContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(
                databaseContext,
                this.excludedMainUserAccountId,
                100,
                "chat-user",
                "chat-user@example.com");

            await ApiTestDataBuilder.SeedUserAsync(
                databaseContext,
                this.secondaryTestUserAccountId,
                101,
                "second-user",
                "second@example.com");

            await ApiTestDataBuilder.SeedUserAsync(
                databaseContext,
                this.tertiaryTestUserAccountId,
                102,
                "third-user",
                "third@example.com");
        }

        [TearDown]
        public void TearDown_TestResources()
        {
            this.apiHttpClient.Dispose();
            this.apiTestApplicationFactory.Dispose();
        }

        [Test]
        public async Task GetUsersExcept_ReturnsAllOtherUsers()
        {
            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!.Length, Is.EqualTo(5));

            Assert.That(returnedUsers.Any(returnedUser => returnedUser.Id == this.secondaryTestUserAccountId), Is.True);
            Assert.That(returnedUsers.Any(returnedUser => returnedUser.Id == this.tertiaryTestUserAccountId), Is.True);
            Assert.That(returnedUsers.Any(returnedUser => returnedUser.Id == this.excludedMainUserAccountId), Is.False);
        }

        [Test]
        public async Task GetUsersExcept_WhenExcludingSingleUser_ReturnsRemainingUsers()
        {
            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.secondaryTestUserAccountId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!.All(returnedUser => returnedUser.Id != this.secondaryTestUserAccountId), Is.True);
        }

        [Test]
        public async Task GetUsersExcept_WhenExcludingAllUsers_ReturnsEmptyResultSet()
        {
            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!.All(returnedUser => returnedUser.Id != this.excludedMainUserAccountId), Is.True);
        }

        [Test]
        public async Task GetUsersExcept_MultipleRequestsReturnConsistentResults()
        {
            var firstResponse = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            var secondResponse = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            firstResponse.EnsureSuccessStatusCode();
            secondResponse.EnsureSuccessStatusCode();

            var firstUsers = await firstResponse.Content.ReadFromJsonAsync<UserDTO[]>();
            var secondUsers = await secondResponse.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(firstUsers!.Length, Is.EqualTo(secondUsers!.Length));
        }

        [Test]
        public async Task GetUsersExcept_WithNonExistentUserId_ReturnsAllUsers()
        {
            var nonExistentUserId = Guid.NewGuid();

            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{nonExistentUserId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!.Length, Is.EqualTo(6));
        }

        [Test]
        public async Task GetUsersExcept_ReturnsValidUserStructure()
        {
            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            var firstUser = returnedUsers!.First();

            Assert.That(firstUser.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(firstUser.DisplayName, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task GetUsersExcept_ExcludingSameUserMultipleTimes_StillWorks()
        {
            var response = await this.apiHttpClient.GetAsync(
                $"api/users/except/{this.excludedMainUserAccountId}");

            response.EnsureSuccessStatusCode();

            var returnedUsers = await response.Content.ReadFromJsonAsync<UserDTO[]>();

            Assert.That(returnedUsers, Is.Not.Null);
            Assert.That(returnedUsers!.All(returnedUser => returnedUser.Id != this.excludedMainUserAccountId), Is.True);
        }
    }
}
