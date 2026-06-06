// <copyright file="ListingsViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using BoardGames.Desktop.Commands;
using AppConstants = BoardGames.Desktop.Constants.Constants;

namespace BoardGames.Desktop.ViewModels
{
    using System;
    using System.Collections.Immutable;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardGames.Desktop.Services;
    using BoardGames.Shared.DTO;
    using BoardGames.Shared.ProxyServices;
    using BoardGames.Desktop.Commands;
    using AppConstants = BoardGames.Desktop.Constants.Constants;
    using Microsoft.UI.Xaml.Media.Imaging;

    public class ListingsViewModel : PagedViewModel<ListingGameCardViewModel>
    {
        private const int NoActiveRentalsCount = 0;
        private const string DeleteSuccessMessageTemplate = "There are {0} active rentals for this game. It was removed successfully.";

        private readonly IGameService gameListingService;
        private readonly IDesktopAuthorizationService authorizationService;
        private readonly Uri apiBaseUri;

        public ListingsViewModel(IGameService gameListingService, IDesktopAuthorizationService authorizationService)
        {
            this.gameListingService = gameListingService;
            this.authorizationService = authorizationService;
            this.apiBaseUri = ResolveApiBaseUri();
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
                SetAllItems(ImmutableList<ListingGameCardViewModel>.Empty);
                return;
            }

            var gameListingsResult = authorizationService.IsAdministrator && !showOnlyMyGames
                ? await gameListingService.GetAllGamesAsync()
                : await gameListingService.GetGamesForOwnerAsync(authorizationService.CurrentAccountId);

            this.SetAllItems(gameListingsResult.Success && gameListingsResult.Data != null
                ? gameListingsResult.Data.Select(game => new ListingGameCardViewModel(game, apiBaseUri))
                : ImmutableList<ListingGameCardViewModel>.Empty);
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} games";

        public async Task DeleteGameAsync(ListingGameCardViewModel gameToDelete)
        {
            if (!CanManageGame(gameToDelete.Game))
                throw new UnauthorizedAccessException("You are not authorized to delete this game.");
            }

            var deleteResult = await gameListingService.DeleteGameAsync(gameToDelete.Game.Id);
            if (!deleteResult.Success)
            {
                throw new InvalidOperationException(deleteResult.Error ?? "Unexpected error occurred.");
            }

            await ReloadAsync();
        }

        public async Task<ViewOperationResult> TryDeleteGameAsync(ListingGameCardViewModel gameToDelete)
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

        private static Uri ResolveApiBaseUri()
        {
            string? configuredBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.Trim();

            if (string.IsNullOrWhiteSpace(configuredBaseUrl) || !Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException("ApiBaseUrl is not configured correctly in App.config.");
            }

            return baseUri;
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

    public sealed class ListingGameCardViewModel
    {
        private static readonly Uri FallbackImageUri = new("ms-appx:///Assets/default-game-placeholder.jpg");

        public ListingGameCardViewModel(GameSummaryDTO game, Uri apiBaseUri)
        {
            Game = game;
            Name = game.Name;
            DetailsText = $"Price: {game.Price:0.##} RON/day   Players: {game.MinimumPlayerNumber} - {game.MaximumPlayerNumber}";
            OwnerText = $"Owner: {GetOwnerDisplayName(game.OwnerDisplayName)}";
            StatusText = game.IsActive ? "Active" : "Inactive";
            ImageSource = CreateImage(game.ImageUrl, apiBaseUri);
        }

        public GameSummaryDTO Game { get; }

        public string Name { get; }

        public string DetailsText { get; }

        public string OwnerText { get; }

        public string StatusText { get; }

        public BitmapImage ImageSource { get; }

        private static BitmapImage CreateImage(string imageUrl, Uri apiBaseUri)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return new BitmapImage(FallbackImageUri);
                }

                var imageUri = Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri)
                    ? absoluteUri
                    : new Uri(apiBaseUri, imageUrl.TrimStart('/'));

                return new BitmapImage(imageUri);
            }
            catch
            {
                return new BitmapImage(FallbackImageUri);
            }
        }

        private static string GetOwnerDisplayName(string ownerDisplayName)
        {
            return string.IsNullOrWhiteSpace(ownerDisplayName)
                ? "BoardGames host"
                : ownerDisplayName;
        }
    }
}
