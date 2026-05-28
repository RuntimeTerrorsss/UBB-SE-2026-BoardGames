// <copyright file="NotificationsApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class NotificationsApiIntegrationTests
    {
        private readonly Guid notificationRecipientAccountId = Guid.NewGuid();

        private ApiWebApplicationFactory webApplicationFactory = null!;
        private HttpClient apiHttpClient = null!;

        private int persistedNotificationId;

        [SetUp]
        public async Task SetUp()
        {
            this.webApplicationFactory = new ApiWebApplicationFactory();

            await this.webApplicationFactory.EnsureDatabaseAsync();

            this.apiHttpClient = this.webApplicationFactory.CreateClient();
            using var serviceScope = this.webApplicationFactory.Services.CreateScope();

            var applicationDbContext = serviceScope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(
                applicationDbContext,
                this.notificationRecipientAccountId,
                50,
                "notify-user",
                "notify-user@example.com");

            var seededNotification = new Notification
            {
                Recipient = applicationDbContext.Users.First(
                    user => user.Id == this.notificationRecipientAccountId),

                Timestamp = DateTime.UtcNow,
                Title = "Initial Title",
                Body = "Initial Body",
                Type = BoardGames.Data.Enums.NotificationType.Informational,
            };

            applicationDbContext.Notifications.Add(seededNotification);

            await applicationDbContext.SaveChangesAsync();

            this.persistedNotificationId = seededNotification.Id;
        }

        [TearDown]
        public void TearDown()
        {
            this.apiHttpClient.Dispose();
            this.webApplicationFactory.Dispose();
        }

        [Test]
        public async Task GetForUser_WithExistingNotifications_ReturnsNotifications()
        {
            var getNotificationsForUserHttpResponse =
                await this.apiHttpClient.GetAsync(
                    $"api/notifications/user/{this.notificationRecipientAccountId}");

            Assert.That(
                getNotificationsForUserHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            var returnedNotifications =
                await getNotificationsForUserHttpResponse.Content
                    .ReadFromJsonAsync<NotificationDTO[]>();

            Assert.That(returnedNotifications, Is.Not.Null);

            Assert.That(
                returnedNotifications!.Length,
                Is.GreaterThan(0));

            Assert.That(
                returnedNotifications.Any(
                    notification => notification.Id == this.persistedNotificationId),
                Is.True);
        }

        [Test]
        public async Task GetById_WithExistingNotification_ReturnsNotification()
        {
            var getNotificationByIdHttpResponse =
                await this.apiHttpClient.GetAsync(
                    $"api/notifications/{this.persistedNotificationId}");

            Assert.That(
                getNotificationByIdHttpResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            var returnedNotification =
                await getNotificationByIdHttpResponse.Content
                    .ReadFromJsonAsync<NotificationDTO>();

            Assert.That(returnedNotification, Is.Not.Null);

            Assert.That(
                returnedNotification!.Id,
                Is.EqualTo(this.persistedNotificationId));

            Assert.That(
                returnedNotification.Title,
                Is.EqualTo("Initial Title"));
        }
    }
}
