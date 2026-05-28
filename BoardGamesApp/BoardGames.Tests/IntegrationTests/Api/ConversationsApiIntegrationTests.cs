using System;
using System.Collections.Generic;
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
    public sealed class ConversationsApiIntegrationTests
    {
        private readonly Guid senderAccountId = Guid.NewGuid();
        private readonly Guid receiverAccountId = Guid.NewGuid();
        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            factory = new ApiWebApplicationFactory();
            await factory.EnsureDatabaseAsync();
            client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ApiTestDataBuilder.SeedUserAsync(dbContext, senderAccountId, 80, "chat-sender", "chat-sender@example.com");
            await ApiTestDataBuilder.SeedUserAsync(dbContext, receiverAccountId, 81, "chat-receiver", "chat-receiver@example.com");
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task CreateConversation_ThenGetById_ReturnsConversation()
        {
            var request = new { SenderAccountId = senderAccountId, ReceiverAccountId = receiverAccountId };
            var createResponse = await client.PostAsJsonAsync("api/conversation", request);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ConversationDTO>();

            Assert.That(created, Is.Not.Null);
            Assert.That(created!.Id, Is.GreaterThan(0));

            var getResponse = await client.GetAsync($"api/conversation/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<ConversationDTO>();

            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task SendMessage_ReturnsPersistedMessage()
        {
            var request = new { SenderAccountId = senderAccountId, ReceiverAccountId = receiverAccountId };
            var createResponse = await client.PostAsJsonAsync("api/conversation", request);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ConversationDTO>();

            var message = new MessageDataTransferObject(
                Id: 0,
                ConversationId: created!.Id,
                SenderId: 80,
                ReceiverId: 81,
                SentAt: DateTime.UtcNow,
                Content: "Hello",
                Type: MessageType.MessageText,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                RequestId: -1,
                PaymentId: -1);

            var sendResponse = await client.PostAsJsonAsync("api/conversation/messages", message);
            sendResponse.EnsureSuccessStatusCode();
            var persisted = await sendResponse.Content.ReadFromJsonAsync<MessageDataTransferObject>();

            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted!.Content, Is.EqualTo("Hello"));
        }
    }
}
