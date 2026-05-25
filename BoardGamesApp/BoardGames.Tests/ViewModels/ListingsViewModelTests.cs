using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardGames.Shared.DTO;
using BoardRentAndProperty.ViewModels;
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
            gameService = new FakeClientGameService();
        }

        [Test]
        public void Constructor_LoadsGamesForOwner()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2), BuildGame(3));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_NoGames_TotalCountIsZero()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void ShowingText_ContainsGameCountAndGamesWord()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("2"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        [Test]
        public async Task LoadGames_RefreshesCollectionFromService()
        {
            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(0));

            gameService.GamesForOwner = ImmutableList.Create(BuildGame(10), BuildGame(11));

            await viewModel.LoadGamesAsync();

            Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteGame_CallsServiceDeleteWithCorrectId()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(42));

            var viewModel = BuildViewModel();
            GameDTO gameToDelete = viewModel.PagedItems.First();

            await viewModel.DeleteGameAsync(gameToDelete);

            Assert.That(gameService.DeleteGameCallCount, Is.EqualTo(1));
            Assert.That(gameService.LastDeletedGameId, Is.EqualTo(42));
        }

        [Test]
        public async Task DeleteGame_ReloadsListAfterDeletion()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1), BuildGame(2));

            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(2));

            gameService.GamesForOwner = ImmutableList.Create(BuildGame(2));

            await viewModel.DeleteGameAsync(BuildGame(1));

            Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task TryDeleteGame_SuccessfulDeletion_ReturnsSuccessWithGameRemovedTitle()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));

            var viewModel = BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(BuildGame(1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DialogTitle, Is.EqualTo("Game Removed"));
        }

        [Test]
        public async Task TryDeleteGame_GameHasActiveRentals_ReturnsFailureWithCannotDeleteTitle()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            gameService.DeleteGameException =
                new InvalidOperationException("There are 2 active rentals for this game and it cannot be removed now.");

            var viewModel = BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Does.Contain("active rentals"));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithMessage_ReturnsFailureWithThatMessage()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            gameService.DeleteGameException = new Exception("Database connection failed.");

            var viewModel = BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Is.EqualTo("Database connection failed."));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithEmptyMessage_ReturnsFallbackMessage()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            gameService.DeleteGameException = new Exception(string.Empty);

            var viewModel = BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public async Task TryDeleteGame_UnexpectedExceptionWithWhitespaceMessage_ReturnsFallbackMessage()
        {
            gameService.GamesForOwner = ImmutableList.Create(BuildGame(1));
            gameService.DeleteGameException = new Exception("   ");

            var viewModel = BuildViewModel();
            ViewOperationResult result = await viewModel.TryDeleteGameAsync(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void PagedItems_MoreGamesThanPageSize_ShowsOnlyFirstPage()
        {
            int pageSize = PagedViewModel<GameDTO>.PageSize;
            var games = Enumerable.Range(1, pageSize + 2).Select(BuildGame).ToImmutableList();
            gameService.GamesForOwner = games;

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(pageSize + 2));
            Assert.That(viewModel.PagedItems.Count, Is.LessThanOrEqualTo(pageSize));
        }

        [Test]
        public void ShowingText_WithGames_IncludesDisplayedAndTotalCounts()
        {
            var games = Enumerable.Range(1, 5).Select(BuildGame).ToImmutableList();
            gameService.GamesForOwner = games;

            var viewModel = BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("5"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        private ListingsViewModel BuildViewModel()
        {
            return new ListingsViewModel(gameService, ownerUserId);
        }

        private GameDTO BuildGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = ownerUserId },
                Name = $"Game {gameId}",
                Price = 9.99m,
                IsActive = true,
                Description = "Test game description.",
            };
        }
    }
}
