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

            using var scope = this.webApplicationFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(
                dbContext,
                this.notificationRecipientAccountId,
                50,
                "notify-user",
                "notify-user@example.com");

            var seededNotificationEntity = new Notification
            {
                Recipient = dbContext.Users.First(user => user.Id == this.notificationRecipientAccountId),
                Timestamp = DateTime.UtcNow,
                Title = "Initial Title",
                Body = "Initial Body",
                Type = BoardGames.Data.Enums.NotificationType.Informational,
            };

            dbContext.Notifications.Add(seededNotificationEntity);
            await dbContext.SaveChangesAsync();

            this.persistedNotificationId = seededNotificationEntity.Id;
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
            var getUserNotificationsResponse =
                await this.apiHttpClient.GetAsync(
                    $"api/notifications/user/{this.notificationRecipientAccountId}");

            Assert.That(getUserNotificationsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var userNotifications =
                await getUserNotificationsResponse.Content
                    .ReadFromJsonAsync<NotificationDTO[]>();

            Assert.That(userNotifications, Is.Not.Null);
            Assert.That(userNotifications!.Any(userNotifications => userNotifications.Id == this.persistedNotificationId), Is.True);
        }

        [Test]
        public async Task GetById_WithExistingNotification_ReturnsNotification()
        {
            var getNotificationByIdResponse =
                await this.apiHttpClient.GetAsync(
                    $"api/notifications/{this.persistedNotificationId}");

            Assert.That(getNotificationByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var notificationById =
                await getNotificationByIdResponse.Content.ReadFromJsonAsync<NotificationDTO>();

            Assert.That(notificationById, Is.Not.Null);
            Assert.That(notificationById!.Id, Is.EqualTo(this.persistedNotificationId));
            Assert.That(notificationById.Title, Is.EqualTo("Initial Title"));
        }

        [Test]
        public async Task GetById_WithMissingNotification_ReturnsNotFound()
        {
            var getMissingNotificationResponse =
                await this.apiHttpClient.GetAsync($"api/notifications/{int.MaxValue}");

            Assert.That(getMissingNotificationResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Update_WithValidNotification_ReturnsNoContent()
        {
            var updateNotificationDto = new NotificationDTO
            {
                Id = this.persistedNotificationId,
                Title = "Updated Title",
                Body = "Updated Body",
                Timestamp = DateTime.UtcNow,
                Recipient = new UserDTO { DisplayName = "Test" },
            };

            var updateNotificationResponse =
                await this.apiHttpClient.PutAsJsonAsync(
                    $"api/notifications/{this.persistedNotificationId}",
                    updateNotificationDto);

            Assert.That(updateNotificationResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Update_WithMissingNotification_ReturnsNotFound()
        {
            var updateMissingNotificationDto = new NotificationDTO
            {
                Id = int.MaxValue,
                Title = "X",
                Body = "Y",
                Timestamp = DateTime.UtcNow,
                Recipient = new UserDTO { DisplayName = "Test" },
            };

            var updateMissingNotificationResponse =
                await this.apiHttpClient.PutAsJsonAsync(
                    $"api/notifications/{int.MaxValue}",
                    updateMissingNotificationDto);

            Assert.That(updateMissingNotificationResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Delete_WithExistingNotification_ReturnsOk()
        {
            var deleteNotificationResponse =
                await this.apiHttpClient.DeleteAsync(
                    $"api/notifications/{this.persistedNotificationId}");

            Assert.That(deleteNotificationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_WithMissingNotification_ReturnsNotFound()
        {
            var deleteMissingNotificationResponse =
                await this.apiHttpClient.DeleteAsync(
                    $"api/notifications/{int.MaxValue}");

            Assert.That(deleteMissingNotificationResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetForUser_MultipleCalls_ReturnsConsistentResults()
        {
            var firstUserNotificationsResponse =
                await this.apiHttpClient.GetFromJsonAsync<NotificationDTO[]>(
                    $"api/notifications/user/{this.notificationRecipientAccountId}");

            var secondUserNotificationsResponse =
                await this.apiHttpClient.GetFromJsonAsync<NotificationDTO[]>(
                    $"api/notifications/user/{this.notificationRecipientAccountId}");

            Assert.That(
                firstUserNotificationsResponse!.Length,
                Is.EqualTo(secondUserNotificationsResponse!.Length));
        }
    }
}