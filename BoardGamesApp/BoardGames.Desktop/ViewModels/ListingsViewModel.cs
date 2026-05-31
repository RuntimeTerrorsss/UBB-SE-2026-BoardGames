// <copyright file="ListingsViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using BoardGames.Desktop.Services;
    using BoardGames.Shared.DTO;
    using BoardGames.Shared.ProxyServices;
    using BoardGames.Desktop.Commands;
    using AppConstants = BoardGames.Desktop.Constants.Constants;

    public class ListingsViewModel : PagedViewModel<GameSummaryDTO>
    {
        private const int NoActiveRentalsCount = 0;
        private const string DeleteSuccessMessageTemplate = "There are {0} active rentals for this game. It was removed successfully.";

        private readonly IGameService gameListingService;
        private readonly IDesktopAuthorizationService authorizationService;
        private bool showOnlyMyGames;

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

        public string PageTitle => authorizationService.IsAdministrator ? "Games" : "My Games";

        public bool IsAdministrator => authorizationService.IsAdministrator;

        public bool ShowOnlyMyGames
        {
            get => showOnlyMyGames;
            set
            {
                if (showOnlyMyGames != value)
                {
                    showOnlyMyGames = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilterButtonLabel));
                    _ = ReloadAsync();
                }
            }
        }

        public string FilterButtonLabel => showOnlyMyGames ? "Show all games" : "Show only my games";

        public Task LoadGamesAsync() => ReloadAsync();

        public void ToggleMyGamesFilter()
        {
            ShowOnlyMyGames = !ShowOnlyMyGames;
        }

        protected override void Reload() => _ = ReloadAsync();

        private async Task ReloadAsync()
        {
            if (!authorizationService.IsLoggedIn)
            {
                SetAllItems(ImmutableList<GameSummaryDTO>.Empty);
                return;
            }

            var gameListingsResult = authorizationService.IsAdministrator && !showOnlyMyGames
                ? await gameListingService.GetAllGamesAsync()
                : await gameListingService.GetGamesForOwnerAsync(authorizationService.CurrentAccountId);

            this.SetAllItems(gameListingsResult.Success && gameListingsResult.Data != null
                ? gameListingsResult.Data
                : ImmutableList<GameSummaryDTO>.Empty);
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public async Task DeleteGameAsync(GameSummaryDTO gameToDelete)
        {
            if (!CanManageGame(gameToDelete))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var deleteResult = await gameListingService.DeleteGameAsync(gameToDelete.Id);
            if (!deleteResult.Success)
            {
                throw new InvalidOperationException(deleteResult.Error ?? "Unexpected error occurred.");
            }

            await ReloadAsync();
        }

        public async Task<ViewOperationResult> TryDeleteGameAsync(GameSummaryDTO gameToDelete)
        {
            try
            {
                await DeleteGameAsync(gameToDelete);
                return ViewOperationResult.Success(
                    AppConstants.DialogTitles.GameRemoved,
                    string.Format(DeleteSuccessMessageTemplate, NoActiveRentalsCount));
            }
            catch (Exception ex)
            {
                return ViewOperationResult.Failure(
                    AppConstants.DialogTitles.CannotDeleteGame,
                    ex.Message);
            }
        }

        private bool CanManageGame(GameSummaryDTO gameToManage)
        {
            return authorizationService.IsAdministrator
                || gameToManage.OwnerAccountId == authorizationService.CurrentAccountId;
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

            public bool CanAccessRoute(AppPage page) => true;
        }
    }
}
