// <copyright file="DeliveryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels
{
    public class DeliveryViewModel
    {
        private const int DefaultUserId = 1;

        public DeliveryViewModel(
            int currentUserId,
            IMapService mapService,
            IUserRepository userRepository,
            IValidator<Dictionary<string, string>, Address> validator)
        {
            CurrentId = currentUserId;
            MapService = mapService;
            UserRepository = userRepository;
            Validator = validator;
            CurrentAddress = new Address();
        }

        public async Task InitializeAsync()
        {
            CurrentUser = await UserRepository.GetById(CurrentId);

            CurrentAddress = CurrentUser != null
                ? new Address(CurrentUser.Country, CurrentUser.City, CurrentUser.Street, CurrentUser.StreetNumber)
                : new Address();

            StateChanged?.Invoke();
        }

        public event Action StateChanged;

        public Address CurrentAddress { get; set; }

        public bool IsMapVisible { get; set; }

        public bool IsSaveAddress { get; set; }

        public Dictionary<string, string> ValidationErrors { get; set; } = new Dictionary<string, string>();

        public User CurrentUser { get; set; }

        public int CurrentId { get; set; } = DefaultUserId;

        public Action OnNavigateToPayment { get; set; }

        private IMapService MapService { get; set; }

        private IUserRepository UserRepository { get; set; }

        private IValidator<Dictionary<string, string>, Address> Validator { get; set; }

        public async Task Initialize(int userId)
        {
            CurrentId = userId;

            CurrentUser = await UserRepository.GetById(userId);

            if (CurrentUser != null)
            {
                await UserRepository.GetById(CurrentUser.Id);

                CurrentAddress = new Address(
                    CurrentUser.Country,
                    CurrentUser.City,
                    CurrentUser.Street,
                    CurrentUser.StreetNumber);
            }
            else
            {
                CurrentAddress = new Address();
            }

            StateChanged?.Invoke();
        }

        public void OnFieldChange(string fieldName, string newValue)
        {
            typeof(Address).GetProperty(fieldName)?.SetValue(CurrentAddress, newValue);

            if (ValidationErrors.Remove(fieldName))
            {
                StateChanged?.Invoke();
            }
        }

        public void OpenMap()
        {
            IsMapVisible = true;
            StateChanged?.Invoke();
        }

        public void CloseMap()
        {
            IsMapVisible = false;
            StateChanged?.Invoke();
        }

        public async Task ConfirmMapLocationAsync(double latitude, double longitude)
        {
            Address resolved = await MapService.GetAddressFromMapAsync(latitude, longitude);

            if (resolved != null)
            {
                CurrentAddress = resolved;
                IsMapVisible = false;
                StateChanged?.Invoke();

                await SaveAddressIfRequestedAsync();
            }
        }

        public async Task SubmitDelivery()
        {
            ValidationErrors = Validator.Validate(CurrentAddress);
            StateChanged?.Invoke();

            if (ValidationErrors.Count == 0)
            {
                await SaveAddressIfRequestedAsync();
                OnNavigateToPayment?.Invoke();
            }
        }

        public void OnSaveAddressChanged(bool isChecked)
        {
            IsSaveAddress = isChecked;

            if (isChecked && CurrentUser != null)
            {
                _ = SaveAddressIfRequestedAsync();
            }
        }

        private async Task SaveAddressIfRequestedAsync()
        {
            if (IsSaveAddress && CurrentUser != null)
            {
                await UserRepository.SaveAddress(CurrentUser.Id, CurrentAddress);

                CurrentUser = await UserRepository.GetById(CurrentUser.Id);
                CurrentAddress = new Address(
                    CurrentUser.Country,
                    CurrentUser.City,
                    CurrentUser.Street,
                    CurrentUser.StreetNumber);

                StateChanged?.Invoke();
            }
        }
    }
}
