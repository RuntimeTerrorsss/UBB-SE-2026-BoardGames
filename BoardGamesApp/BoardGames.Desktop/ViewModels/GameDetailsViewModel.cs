// <copyright file="GameDetailsViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using BoardGames.Api.Mappers;
using BoardGames.Shared.ProxyServices;
using BoardGames.Desktop.Commands;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BoardGames.Desktop.ViewModels
{
    /// <summary>
    /// Provides details for a specific game, including pricing, availability, and booking commands.
    /// </summary>
    public class GameDetailsViewModel : INotifyPropertyChanged
    {
        private const long UnregisteredUserID = -1;
        private const decimal DefaultTotalPrice = 0;
        private readonly InterfaceBookingService bookingService;
        private bool hasError;
        private decimal totalPrice;
        private BitmapImage? gameImage;
        private string? ownerImageUrl;
        private BookingDTO gameAndUserDetail;
        private int gameId;

        public event Action<int, int>? OnChatWithOwnerRequested;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameDetailsViewModel"/> class.
        /// </summary>
        /// <param name="bookingService">The service used for booking operations and data retrieval.</param>
        /// <param name="gameId">The unique identifier of the game to display details for.</param>
        public GameDetailsViewModel(InterfaceBookingService bookingService, int gameId)
        {

            this.bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            gameAndUserDetail = new BookingDTO();
            this.gameId = gameId;
        }

        public async Task InitializeAsync()
        {
            try
            {
                GameAndUserDetails = await bookingService.GetBookingInformationForSpecificGame(gameId);
                UnavailableTimeRanges = await bookingService.GetUnavailableTimeRanges(gameId) ?? Array.Empty<TimeRange>();
                LoadGameImage();
                LoadOwnerImage();
                HasError = false;
            }
            catch (Exception exception)
            {
                HasError = true;
                UnavailableTimeRanges = Array.Empty<TimeRange>();
                OnMessageRequested?.Invoke($"Could not load game details. {exception.Message}");
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when a navigation back request is made.
        /// </summary>
        public event Action? OnGoBackRequested;

        /// <summary>
        /// Occurs when the user requests to start the booking process for a specific game and time range.
        /// </summary>
        public event Action<BookingDTO, TimeRange>? OnStartBookingRequested;

        /// <summary>
        /// Occurs when a message or notification needs to be displayed to the user.
        /// </summary>
        public event Action<string>? OnMessageRequested;

        /// <summary>
        /// Gets the current date.
        /// </summary>
        public DateTimeOffset Today => DateTimeOffset.Now.Date;

        /// <summary>
        /// Gets the combined details of the game and its owner.
        /// </summary>
        public BookingDTO GameAndUserDetails
        {
            get => gameAndUserDetail;
            private set
            {
                gameAndUserDetail = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the view model is in an error state.
        /// </summary>
        public bool HasError
        {
            get => hasError;
            private set
            {
                hasError = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the total calculated price for the current selection.
        /// </summary>
        public decimal TotalPrice
        {
            get => totalPrice;
            private set
            {
                totalPrice = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the image of the game.
        /// </summary>
        public BitmapImage? GameImage
        {
            get => gameImage;
            private set
            {
                gameImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the URL of the owner's profile image.
        /// </summary>
        public string? OwnerImageUrl
        {
            get => ownerImageUrl;
            private set
            {
                ownerImageUrl = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the array of time ranges when the game is already booked.
        /// </summary>
        public TimeRange[] UnavailableTimeRanges { get; private set; } = Array.Empty<TimeRange>();

        /// <summary>
        /// Gets the command to navigate back to the previous view.
        /// </summary>
        public ICommand GoBackCommand => new RelayCommand(_ => GoBack());

        /// <summary>
        /// Gets the command to initiate the booking process for the selected game.
        /// </summary>
        public ICommand BookCommand => new RelayCommand(commandParameter =>
        {
            try
            {
                if (commandParameter is TimeRange timeRange)
                {
                    StartBooking(timeRange);
                }
                else
                {
                    OnMessageRequested?.Invoke("Invalid booking interval selected.");
                }
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not start booking. {exception.Message}");
            }
        });

        /// <summary>
        /// Gets the command to initiate the booking process for the selected game.
        /// </summary>
        public ICommand ChatWithOwnerCommand => new RelayCommand(_ =>
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            OnChatWithOwnerRequested?.Invoke(currentUserId, GameAndUserDetails.UserId);
        });

        /// <summary>
        /// Checks if the game is available for a given time range.
        /// </summary>
        /// <param name="timeRange">The period to check for availability.</param>
        /// <returns>True if available; otherwise, false.</returns>
        public async Task<bool> CheckGameAvailability(TimeRange timeRange)
        {
            try
            {
                if (timeRange == null)
                {
                    return false;
                }

                return await bookingService.CheckGameAvailability(GameAndUserDetails.GameId, timeRange);
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not check availability. {exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// Calculates the price for a specific booking duration.
        /// </summary>
        /// <param name="timeRange">The booking duration.</param>
        /// <returns>The total calculated price.</returns>
        public decimal CalculatePrice(TimeRange timeRange)
        {
            try
            {
                if (timeRange == null)
                {
                    throw new ArgumentNullException(nameof(timeRange));
                }

                TotalPrice = bookingService.CalculateTotalPriceForRentingASpecificGame(GameAndUserDetails.Price, timeRange);
                return TotalPrice;
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not calculate price. {exception.Message}");
                TotalPrice = DefaultTotalPrice;
                return DefaultTotalPrice;
            }
        }

        /// <summary>
        /// Initiates the booking flow for the selected game.
        /// </summary>
        /// <param name="timeRange">The chosen time range for the reservation.</param>
        public void StartBooking(TimeRange timeRange)
        {
            try
            {
                if (SessionContext.GetInstance().UserId == UnregisteredUserID)
                {
                    OnMessageRequested?.Invoke("User not logged in. Please log in first");

                    // TODO login
                    return;
                }

                if (timeRange == null)
                {
                    OnMessageRequested?.Invoke("Please select a valid booking timeRange.");
                    return;
                }

                OnStartBookingRequested?.Invoke(GameAndUserDetails, timeRange);
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not continue to booking. {exception.Message}");
            }
        }

        /// <summary>
        /// Triggers the go-back navigation logic.
        /// </summary>
        public void GoBack()
        {
            try
            {
                OnGoBackRequested?.Invoke();
            }
            catch (Exception exception)
            {
                OnMessageRequested?.Invoke($"Could not go back. {exception.Message}");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
           => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void LoadGameImage()
        {
            try
            {
                if (GameAndUserDetails.Image != null && GameAndUserDetails.Image.Length > 0)
                {
                    GameImage = await Helpers.GameImage.ToBitmapImageAsync(GameAndUserDetails.Image);
                }
                else
                {
                    var imageUrl = GameImageMapper.GetImageUrl(GameAndUserDetails.Name);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        GameImage = new BitmapImage(new Uri(imageUrl));
                    }
                }
            }
            catch (Exception exception)
            {
                GameImage = null;
                OnMessageRequested?.Invoke($"Could not load game image. {exception.Message}");
            }
        }

        private void LoadOwnerImage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GameAndUserDetails.AvatarUrl))
                {
                    OwnerImageUrl = null;
                    return;
                }

                OwnerImageUrl = GameAndUserDetails.AvatarUrl;
            }
            catch (Exception exception)
            {
                OwnerImageUrl = null;
                OnMessageRequested?.Invoke($"Could not load owner image. {exception.Message}");
            }
        }
    }
}
