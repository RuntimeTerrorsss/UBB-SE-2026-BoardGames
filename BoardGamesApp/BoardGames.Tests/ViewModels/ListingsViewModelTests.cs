using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Tests.Fakes;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class ListingsViewModelTests
    {
        private FakeClientGameService gameService = null!;
        private Mock<IDesktopAuthorizationService> mockAuthService = null!;
        private readonly Guid ownerUserId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService();
            this.mockAuthService = new Mock<IDesktopAuthorizationService>();
            this.mockAuthService.Setup(a => a.IsLoggedIn).Returns(true);
            this.mockAuthService.Setup(a => a.CurrentAccountId).Returns(this.ownerUserId);
        }

        [Test]
        public async Task LoadGamesAsync_ShouldLoadGamesForOwner_WhenNotAdmin()
        {
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(false);

            // Folosim proprietatea existentă în FakeClientGameService
            this.gameService.GamesForOwner = ImmutableList.Create(new GameSummaryDTO { Id = 1 });

            var viewModel = new ListingsViewModel(this.gameService, this.mockAuthService.Object);
            await viewModel.LoadGamesAsync();

            Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteGameAsync_ShouldThrowUnauthorized_WhenNotOwnerAndNotAdmin()
        {
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(false);
            var game = new GameSummaryDTO { Id = 1, OwnerAccountId = Guid.NewGuid() };

            var viewModel = new ListingsViewModel(this.gameService, this.mockAuthService.Object);

            Func<Task> action = () => viewModel.DeleteGameAsync(game);
            Assert.ThrowsAsync<UnauthorizedAccessException>(action);
        }

        [Test]
        public async Task DeleteGameAsync_ShouldCallService_WhenUserIsOwner()
        {
            var game = new GameSummaryDTO { Id = 42, OwnerAccountId = this.ownerUserId };
            this.gameService.DeletedGameResult = new GameSummaryDTO { Id = 42 };

            var viewModel = new ListingsViewModel(this.gameService, this.mockAuthService.Object);

            await viewModel.DeleteGameAsync(game);

            Assert.That(this.gameService.DeleteGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastDeletedGameId, Is.EqualTo(42));
        }

        [Test]
        public async Task TryDeleteGameAsync_ShouldReturnSuccess_WhenDeletionSucceeds()
        {
            var game = new GameSummaryDTO { Id = 1, OwnerAccountId = this.ownerUserId };
            this.gameService.DeletedGameResult = new GameSummaryDTO { Id = 1 };

            var viewModel = new ListingsViewModel(this.gameService, this.mockAuthService.Object);
            var result = await viewModel.TryDeleteGameAsync(game);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DialogTitle, Is.EqualTo("Game Removed"));
        }

        [Test]
        public async Task TryDeleteGameAsync_ShouldReturnFailure_WhenServiceFails()
        {
            var game = new GameSummaryDTO { Id = 1, OwnerAccountId = this.ownerUserId };
            this.gameService.DeleteGameException = new Exception("Database error");

            var viewModel = new ListingsViewModel(this.gameService, this.mockAuthService.Object);
            var result = await viewModel.TryDeleteGameAsync(game);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("Database error"));
        }
    }
}
