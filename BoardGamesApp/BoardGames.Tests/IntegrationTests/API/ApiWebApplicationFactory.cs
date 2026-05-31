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
            public Task<List<ConversationDTO>> GetConversationsForUser(Guid accountId) => Task.FromResult(new List<ConversationDTO>());
            public Task<ConversationDTO?> GetConversationById(int conversationId) => Task.FromResult<ConversationDTO?>(null);
            public Task<MessageDataTransferObject> SendMessage(MessageDataTransferObject dto) => Task.FromResult(dto);
            public Task<MessageDataTransferObject?> UpdateMessage(MessageDataTransferObject dto) => Task.FromResult<MessageDataTransferObject?>(dto);
            public Task HandleReadReceipt(BoardGames.Data.Models.ReadReceiptDTO dto) => Task.CompletedTask;
            public Task<int> FindOrCreateConversation(Guid accountIdA, Guid accountIdB) => Task.FromResult(1);
            public Task AttachRentalRequestMessage(int requestId, Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime start, DateTime end) => Task.CompletedTask;
            public Task FinalizeRentalRequestMessage(int requestId, bool accepted) => Task.CompletedTask;
            public Task<MessageDataTransferObject?> CreateCashAgreementMessage(int parentMessageId, int paymentId) => Task.FromResult<MessageDataTransferObject?>(null);
            public Task AcceptRentalRequestMessage(int requestId, int rentalId) => Task.CompletedTask;
        }
    }
}