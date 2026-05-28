using BoardGames.Desktop.ViewModels;
// <copyright file="ListingsViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class ListingsViewModelTests
    {
        private readonly Guid ownerUserId = Guid.NewGuid();
        private FakeClientGameService gameService = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService();
        }

        [Test]
        public void Constructor_LoadsGamesForOwner()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1), this.BuildGame(2), this.BuildGame(3));

            var viewModel = this.BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_NoGames_TotalCountIsZero()
        {
            var viewModel = this.BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void ShowingText_ContainsGameCountAndGamesWord()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1), this.BuildGame(2));

            var viewModel = this.BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("2"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        [Test]
        public async Task LoadGames_RefreshesCollectionFromService()
        {
            var viewModel = this.BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(0));

            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(10), this.BuildGame(11));

            await viewModel.LoadGamesAsync();

            Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteGame_CallsServiceDeleteWithCorrectId()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(42));

            var viewModel = this.BuildViewModel();
            GameDTO gameToDelete = viewModel.PagedItems.First();

            await viewModel.DeleteGameAsync(gameToDelete);

            Assert.That(this.gameService.DeleteGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastDeletedGameId, Is.EqualTo(42));
        }

        [Test]
        public async Task DeleteGame_ReloadsListAfterDeletion()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1), this.BuildGame(2));

            var viewModel = this.BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(2));

            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(2));

            await viewModel.DeleteGameAsync(this.BuildGame(1));

            Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task TryDeleteGame_SuccessfulDeletion_ReturnsSuccessWithGameRemovedTitle()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1));

            var viewModel = this.BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(this.BuildGame(1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DialogTitle, Is.EqualTo("Game Removed"));
        }

        [Test]
        public async Task TryDeleteGame_GameHasActiveRentals_ReturnsFailureWithCannotDeleteTitle()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1));
            this.gameService.DeleteGameException =
                new InvalidOperationException("There are 2 active rentals for this game and it cannot be removed now.");

            var viewModel = this.BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(this.BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Does.Contain("active rentals"));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithMessage_ReturnsFailureWithThatMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1));
            this.gameService.DeleteGameException = new Exception("Database connection failed.");

            var viewModel = this.BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(this.BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Is.EqualTo("Database connection failed."));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithEmptyMessage_ReturnsFallbackMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1));
            this.gameService.DeleteGameException = new Exception(string.Empty);

            var viewModel = this.BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(this.BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithWhitespaceMessage_ReturnsFallbackMessage()
        {
            this.gameService.GamesForOwner = ImmutableList.Create(this.BuildGame(1));
            this.gameService.DeleteGameException = new Exception("   ");

            var viewModel = this.BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(this.BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void PagedItems_MoreGamesThanPageSize_ShowsOnlyFirstPage()
        {
            int pageSize = PagedViewModel<GameDTO>.PageSize;
            var games = Enumerable.Range(1, pageSize + 2).Select(this.BuildGame).ToImmutableList();
            this.gameService.GamesForOwner = games;

            var viewModel = this.BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(pageSize + 2));
            Assert.That(viewModel.PagedItems.Count, Is.LessThanOrEqualTo(pageSize));
        }

        [Test]
        public void ShowingText_WithGames_IncludesDisplayedAndTotalCounts()
        {
            var games = Enumerable.Range(1, 5).Select(this.BuildGame).ToImmutableList();
            this.gameService.GamesForOwner = games;

            var viewModel = this.BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("5"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        private ListingsViewModel BuildViewModel()
        {
            return new ListingsViewModel(this.gameService, this.ownerUserId);
        }

        private GameDTO BuildGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = this.ownerUserId },
                Name = $"Game {gameId}",
                Price = 9.99m,
                IsActive = true,
                Description = "Test game description.",
            };
        }
    }
}
