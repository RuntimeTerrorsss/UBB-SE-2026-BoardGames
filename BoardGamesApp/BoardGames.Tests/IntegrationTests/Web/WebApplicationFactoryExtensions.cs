extern alias WebProject;
using WebProgram = WebProject::Program;
using System;
using System.Linq;
using BoardGames.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace BoardGames.Tests.IntegrationTests.Web
{
    public sealed class WebAppFactory : WebApplicationFactory<WebProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var repoDescriptors = services.Where(descriptor =>
                        descriptor.ServiceType == typeof(InterfaceGamesRepository)
                        || descriptor.ServiceType == typeof(IConversationRepository)
                        || descriptor.ServiceType == typeof(IPaymentRepository)
                        || descriptor.ServiceType == typeof(IRentalRepository)
                        || descriptor.ServiceType == typeof(IRepositoryPayment)
                        || descriptor.ServiceType == typeof(IUserRepository))
                    .ToList();

                foreach (var descriptor in repoDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<InterfaceGamesRepository, StubGamesRepository>();
                services.AddSingleton<IConversationRepository, StubConversationRepository>();
                services.AddSingleton<IPaymentRepository, StubPaymentRepository>();
                services.AddSingleton<IRentalRepository, StubRentalRepository>();
                services.AddSingleton<IRepositoryPayment, StubRepositoryPayment>();
                services.AddSingleton<IUserRepository, StubUserRepository>();
            });
        }
    }
}
