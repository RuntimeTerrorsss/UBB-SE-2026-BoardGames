using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardGames.Desktop.ViewModels
{
    public class ListingsViewModel : PagedViewModel<GameDTO>
    {
        private const int NoActiveRentalsCount = 0;
        private const string DeleteSuccessMessageTemplate =
            "There are {0} active rentals for this game. It was removed successfully.";

        private readonly IGameService gameListingService;
        private readonly IDesktopAuthorizationService authorizationService;

        public ListingsViewModel(IGameService gameListingService, IDesktopAuthorizationService authorizationService)
        {
            this.gameListingService = gameListingService;
            this.authorizationService = authorizationService;
            _ = ReloadAsync();
        }

        public ListingsViewModel(IGameService gameListingService, Guid currentAccountId)
            : this(gameListingService, new FixedDesktopAuthorizationService(currentAccountId))
        {
        }

        public string PageTitle => authorizationService.IsAdministrator ? "Games" : "My Listings";

        public Task LoadGamesAsync() => ReloadAsync();

        protected override void Reload()
        {
            _ = ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            if (!authorizationService.IsLoggedIn)
            {
                SetAllItems(ImmutableList<GameDTO>.Empty);
                return;
            }

            var gameListingsResult = authorizationService.IsAdministrator
                ? await gameListingService.GetAllGamesAsync()
                : await gameListingService.GetGamesForOwnerAsync(authorizationService.CurrentAccountId);

            this.SetAllItems(gameListingsResult.Success && gameListingsResult.Data != null
                ? gameListingsResult.Data.ToImmutableList()
                : ImmutableList<GameDTO>.Empty);
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public async Task DeleteGameAsync(GameDTO gameToDelete)
        {
            if (!CanManageGame(gameToDelete))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var deleteResult = await gameListingService.DeleteGameAsync(gameToDelete.Id);
            if (!deleteResult.Success)
            {
                throw new InvalidOperationException(deleteResult.Error ?? Constants.DialogMessages.UnexpectedErrorOccurred);
            }

            await ReloadAsync();
        }

        public async Task<ViewOperationResult> TryDeleteGameAsync(GameDTO gameToDelete)
        {
            try
            {
                await DeleteGameAsync(gameToDelete);
                return ViewOperationResult.Success(
                    Constants.DialogTitles.GameRemoved,
                    string.Format(DeleteSuccessMessageTemplate, NoActiveRentalsCount));
            }
            catch (InvalidOperationException gameHasActiveRentalsException)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.CannotDeleteGame,
                    gameHasActiveRentalsException.Message);
            }
            catch (Exception unexpectedException)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.CannotDeleteGame,
                    string.IsNullOrWhiteSpace(unexpectedException.Message)
                        ? Constants.DialogMessages.UnexpectedErrorOccurred
                        : unexpectedException.Message);
            }
        }

        private bool CanManageGame(GameDTO gameToManage)
        {
            return authorizationService.IsAdministrator
                || gameToManage.Owner?.Id == authorizationService.CurrentAccountId;
        }

        private sealed class FixedDesktopAuthorizationService : IDesktopAuthorizationService
        {
            private readonly Guid currentAccountId;

            public FixedDesktopAuthorizationService(Guid currentAccountId)
            {
                this.currentAccountId = currentAccountId;
            }

            public Guid CurrentAccountId => currentAccountId;

            public bool IsLoggedIn => true;

            public bool IsAdministrator => false;

            public bool CanAccessPage(Type pageType) => true;

            public bool CanAccessMenuPage(AppPage page) => true;
        }
    }
}
