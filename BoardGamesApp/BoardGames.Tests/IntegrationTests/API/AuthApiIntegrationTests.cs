// <copyright file="AuthApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class AuthApiIntegrationTests
    {
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();
            this.client = this.factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task Register_WithValidData_ReturnsOk()
        {
            var request = new RegisterDTO
            {
                Username = "user1",
                Email = "user1@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "User One",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            var response = await this.client.PostAsJsonAsync("api/auth/register", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Register_WithWeakPassword_ReturnsBadRequest()
        {
            var request = new RegisterDTO
            {
                Username = "user2",
                Email = "user2@mail.com",
                Password = "123",
                ConfirmPassword = "123",
                DisplayName = "User Two",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            var response = await this.client.PostAsJsonAsync("api/auth/register", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Register_WithDuplicateUsername_ReturnsConflict()
        {
            var request = new RegisterDTO
            {
                Username = "duplicateUser",
                Email = "a@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "Duplicate",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            await this.client.PostAsJsonAsync("api/auth/register", request);

            var response2 = await this.client.PostAsJsonAsync("api/auth/register", request);

            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var request1 = new RegisterDTO
            {
                Username = "userA",
                Email = "same@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "User A",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            var request2 = new RegisterDTO
            {
                Username = "userB",
                Email = "same@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "User B",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            await this.client.PostAsJsonAsync("api/auth/register", request1);

            var response2 = await this.client.PostAsJsonAsync("api/auth/register", request2);

            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var register = new RegisterDTO
            {
                Username = "loginUser",
                Email = "login@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "Login User",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            await this.client.PostAsJsonAsync("api/auth/register", register);

            var login = new LoginDTO
            {
                UsernameOrEmail = "loginUser",
                Password = "StrongPass123!",
            };

            var response = await this.client.PostAsJsonAsync("api/auth/login", login);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var register = new RegisterDTO
            {
                Username = "userWrongPass",
                Email = "wrong@mail.com",
                Password = "StrongPass123!",
                ConfirmPassword = "StrongPass123!",
                DisplayName = "Wrong Pass",
                PhoneNumber = "1234567890",
                Country = "Country",
                City = "City",
                StreetName = "Street",
                StreetNumber = "1"
            };

            await this.client.PostAsJsonAsync("api/auth/register", register);

            var login = new LoginDTO
            {
                UsernameOrEmail = "userWrongPass",
                Password = "WRONG",
            };

            var response = await this.client.PostAsJsonAsync("api/auth/login", login);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Login_WithUnknownUser_ReturnsUnauthorized()
        {
            var login = new LoginDTO
            {
                UsernameOrEmail = "doesnotexist",
                Password = "whatever",
            };

            var response = await this.client.PostAsJsonAsync("api/auth/login", login);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Logout_ReturnsNoContent()
        {
            var response = await this.client.PostAsync("api/auth/logout", null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task ForgotPassword_ReturnsOkMessage()
        {
            var response = await this.client.GetAsync("api/auth/forgot-password");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var text = await response.Content.ReadAsStringAsync();

            Assert.That(text, Does.Contain("contact"));
        }
    }
}
