// <copyright file="ConversationsApiIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
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
    public sealed class ConversationsApiIntegrationTests
    {
        private readonly Guid senderId = Guid.NewGuid();
        private readonly Guid receiverId = Guid.NewGuid();

        private ApiWebApplicationFactory factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public async Task SetUp()
        {
            this.factory = new ApiWebApplicationFactory();
            await this.factory.EnsureDatabaseAsync();

            this.client = this.factory.CreateClient();

            using var scope = this.factory.Services.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ApiTestDataBuilder.SeedUserAsync(database, this.senderId, 80, "sender", "sender@test.com");
            await ApiTestDataBuilder.SeedUserAsync(database, this.receiverId, 81, "receiver", "receiver@test.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task CreateConversation_ThenGetById_ReturnsConversation()
        {
            var request = new
            {
                SenderAccountId = this.senderId,
                ReceiverAccountId = this.receiverId,
            };

            var create = await this.client.PostAsJsonAsync("api/conversation", request);
            create.EnsureSuccessStatusCode();

            var conversation = await create.Content.ReadFromJsonAsync<ConversationDTO>();

            Assert.That(conversation, Is.Not.Null);

            var get = await this.client.GetAsync($"api/conversation/{conversation!.Id}");

            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var fetched = await get.Content.ReadFromJsonAsync<ConversationDTO>();

            Assert.That(fetched!.Id, Is.EqualTo(conversation.Id));
        }

        [Test]
        public async Task SendMessage_ReturnsPersistedMessage()
        {
            var request = new
            {
                SenderAccountId = this.senderId,
                ReceiverAccountId = this.receiverId,
            };

            var create = await this.client.PostAsJsonAsync("api/conversation", request);
            var conversation = await create.Content.ReadFromJsonAsync<ConversationDTO>();

            var message = new MessageDataTransferObject(
                Id: 0,
                ConversationId: conversation!.Id,
                SenderId: 80,
                ReceiverId: 81,
                SentAt: DateTime.UtcNow,
                Content: "hello test",
                Type: MessageType.MessageText,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                RequestId: -1,
                PaymentId: -1);

            var send = await this.client.PostAsJsonAsync("api/conversation/messages", message);
            send.EnsureSuccessStatusCode();

            var result = await send.Content.ReadFromJsonAsync<MessageDataTransferObject>();

            Assert.That(result!.Content, Is.EqualTo("hello test"));
        }

        [Test]
        public async Task GetUserConversations_ReturnsEmptyList()
        {
            var result = await this.client.GetAsync($"api/conversation/user/{this.senderId}");

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var conversations = await result.Content.ReadFromJsonAsync<List<ConversationDTO>>();

            Assert.That(conversations, Is.Not.Null);
        }

        [Test]
        public async Task GetMissingConversation_ReturnsNotFound()
        {
            var result = await this.client.GetAsync("api/conversation/999999");

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateMessage_ReturnsOk()
        {
            var message = new MessageDataTransferObject(
                Id: 0,
                ConversationId: 1,
                SenderId: 80,
                ReceiverId: 81,
                SentAt: DateTime.UtcNow,
                Content: "old",
                Type: MessageType.MessageText,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                RequestId: -1,
                PaymentId: -1);

            var sendResult = await this.client.PostAsJsonAsync("api/conversation/messages", message);
            var createdMessage = await sendResult.Content.ReadFromJsonAsync<MessageDataTransferObject>();

            var updatedMessage = createdMessage! with { Content = "new" };

            var result = await this.client.PutAsJsonAsync("api/conversation/messages", updatedMessage);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UpdateMessage_ReturnsNotFound()
        {
            var message = new MessageDataTransferObject(
                Id: 999999,
                ConversationId: 1,
                SenderId: 80,
                ReceiverId: 81,
                SentAt: DateTime.UtcNow,
                Content: "test",
                Type: MessageType.MessageText,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                RequestId: -1,
                PaymentId: -1);

            var result = await this.client.PutAsJsonAsync("api/conversation/messages", message);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task SendReadReceipt_ReturnsNoContent()
        {
            var request = new
            {
                ConversationId = 1,
                ReaderId = 80,
                ReceiverId = 81,
                ReceiptTimeStamp = DateTime.UtcNow,
            };

            var result = await this.client.PostAsJsonAsync("api/conversation/readreceipt", request);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task CreateConversation_ReturnsCreated()
        {
            var request = new
            {
                SenderAccountId = this.senderId,
                ReceiverAccountId = this.receiverId,
            };

            var result = await this.client.PostAsJsonAsync("api/conversation", request);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var body = await result.Content.ReadFromJsonAsync<ConversationDTO>();

            Assert.That(body, Is.Not.Null);
        }

        [Test]
        public async Task CreateCashAgreement_ReturnsNotFound()
        {
            var result = await this.client.PostAsync("api/conversation/cash/1/1", null);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
