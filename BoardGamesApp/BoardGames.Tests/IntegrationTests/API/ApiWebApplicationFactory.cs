using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BoardGames.Tests.IntegrationTests.Api
{
    public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"ApiIntegration_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(descriptor =>
                        descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        descriptor.ServiceType == typeof(AppDbContext) ||
                        descriptor.ServiceType == typeof(IDbContextFactory<AppDbContext>))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                });

                services.AddDbContextFactory<AppDbContext>(
                    options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
                    },
                    ServiceLifetime.Scoped);

                services.AddScoped<IConversationApiService, StubConversationApiService>();

                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.PropertyNameCaseInsensitive = true;
                });
            });
        }

        public async Task EnsureDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        private sealed class StubConversationApiService : IConversationApiService
        {
            private static readonly List<ConversationDTO> conversations = new List<ConversationDTO>();
            private static readonly List<MessageDataTransferObject> messages = new List<MessageDataTransferObject>();
            private static readonly Dictionary<int, (Guid A, Guid B)> conversationParticipants = new Dictionary<int, (Guid A, Guid B)>();
            private static int conversationIdCounter = 1;
            private static int messageIdCounter = 1;

            public Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId)
            {
                var userConversations = conversationParticipants
                    .Where(kvp => kvp.Value.A == accountId || kvp.Value.B == accountId)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                return Task.FromResult(conversations.Where(c => userConversations.Contains(c.Id)).ToList());
            }

            public Task<ConversationDTO?> GetConversationById(int conversationId)
            {
                var found = conversations.FirstOrDefault(c => c.Id == conversationId);
                return Task.FromResult(found);
            }

            public Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto)
            {
                var msg = dto with { Id = messageIdCounter++ };
                messages.Add(msg);
                return Task.FromResult(msg);
            }

            public Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto)
            {
                var index = messages.FindIndex(m => m.Id == dto.Id);
                if (index == -1)
                {
                    return Task.FromResult<MessageDataTransferObject?>(null);
                }

                messages[index] = dto;
                return Task.FromResult<MessageDataTransferObject?>(dto);
            }

            public Task HandleReadReceipt(BoardGames.Data.Models.ReadReceiptDTO dto) => Task.CompletedTask;

            public Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB)
            {
                var existingId = conversationParticipants
                    .Where(kvp => (kvp.Value.A == accountIdA && kvp.Value.B == accountIdB) || (kvp.Value.B == accountIdA && kvp.Value.A == accountIdB))
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault();

                if (existingId != 0)
                {
                    return Task.FromResult(existingId);
                }

                var id = conversationIdCounter++;
                conversationParticipants[id] = (accountIdA, accountIdB);
                conversations.Add(new ConversationDTO
                {
                    Id = id,
                });
                return Task.FromResult(id);
            }

            public Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end) => Task.CompletedTask;
            public Task FinalizeRentalRequestMessage(int requestId, bool accepted) => Task.CompletedTask;
            public Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId) => Task.FromResult<MessageDataTransferObject?>(null);
            public Task AcceptRentalRequestMessage(int requestId, int rentalId) => Task.CompletedTask;
        }
    }
}
