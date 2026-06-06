// <copyright file="ConfirmBookingViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BoardGames.Desktop.ViewModels
{
    public class ConfirmBookingViewModel : INotifyPropertyChanged
    {
        private readonly IRequestService requestService;
        private readonly ISessionContext sessionContext;
        private readonly Uri apiBaseUri;

        private GameDetailDTO game = new();
        private DateTime startDate;
        private DateTime endDate;
        private BitmapImage? gameImage;

        public ConfirmBookingViewModel(IRequestService requestService, ISessionContext sessionContext)
        {
            this.requestService = requestService;
            this.sessionContext = sessionContext;
            this.apiBaseUri = ResolveApiBaseUri();
        }

        public Action? OnGoBackRequested { get; set; }

        public Action? OnConfirmBookingRequested { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action<string>? OnErrorOccurred { get; set; }

        public GameDetailDTO Game
        {
            get => this.game;
            private set
            {
                this.game = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(GameName));
                this.OnPropertyChanged(nameof(OwnerDisplayName));
                this.OnPropertyChanged(nameof(City));
                this.OnPropertyChanged(nameof(PricePerDay));
            }
        }

        public BitmapImage? GameImage
        {
            get => this.gameImage;
            private set
            {
                this.gameImage = value;
                this.OnPropertyChanged();
            }
        }

        public string GameName => this.game.Name;

        public string OwnerDisplayName => this.game.OwnerDisplayName;

        public string City => this.game.City;

        public decimal PricePerDay => this.game.Price;

        public DateTime StartDate
        {
            get => this.startDate;
            private set
            {
                this.startDate = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(FormattedStartDate));
                this.OnPropertyChanged(nameof(NumberOfDays));
                this.OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public DateTime EndDate
        {
            get => this.endDate;
            private set
            {
                this.endDate = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(FormattedEndDate));
                this.OnPropertyChanged(nameof(NumberOfDays));
                this.OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public string FormattedStartDate => this.StartDate.ToString("d MMM yyyy");

        public string FormattedEndDate => this.EndDate.ToString("d MMM yyyy");

        public int NumberOfDays => (this.EndDate.Date - this.StartDate.Date).Days + 1;

        public string TotalPrice => $"{NumberOfDays * this.game.Price:0.##} RON";

        public void Initialize(GameDetailDTO gameDetail, DateTime start, DateTime end)
        {
            this.Game = gameDetail;
            this.StartDate = start.Date;
            this.EndDate = end.Date;
            this.GameImage = CreateImage(gameDetail.ImageUrl, this.apiBaseUri);
        }

        public async Task ConfirmBookingAsync()
        {
            if (!this.sessionContext.IsLoggedIn)
            {
                this.RaiseError("Please sign in before sending a rental request.");
                return;
            }

            if (this.sessionContext.AccountId == this.game.OwnerAccountId)
            {
                this.RaiseError("You cannot rent a game you already own.");
                return;
            }

            var availability = await this.requestService.CheckAvailabilityAsync(this.game.Id, this.StartDate, this.EndDate);
            if (!availability.Success)
            {
                this.RaiseError(availability.Error ?? "Could not check availability.");
                return;
            }

            if (!availability.Data)
            {
                this.RaiseError("The selected dates are unavailable.");
                return;
            }

            var result = await this.requestService.CreateRequestAsync(new CreateRequestDTO
            {
                GameId = this.game.Id,
                RenterAccountId = this.sessionContext.AccountId,
                OwnerAccountId = this.game.OwnerAccountId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
            });

            if (result.Success)
            {
                this.OnConfirmBookingRequested?.Invoke();
            }
            else
            {
                this.RaiseError(result.Error ?? "Failed to submit rental request.");
            }
        }

        public void GoBack() => this.OnGoBackRequested?.Invoke();

        private void RaiseError(string message) => this.OnErrorOccurred?.Invoke(message);

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static BitmapImage? CreateImage(string imageUrl, Uri apiBaseUri)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            try
            {
                var imageUri = Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri)
                    ? absoluteUri
                    : new Uri(apiBaseUri, imageUrl.TrimStart('/'));

                return new BitmapImage(imageUri);
            }
            catch
            {
                return null;
            }
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
    }
}
