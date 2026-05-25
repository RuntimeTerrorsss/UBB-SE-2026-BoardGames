// <copyright file="DeliveryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Sharing.Validators;

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
            this.CurrentId = currentUserId;
            this.MapService = mapService;
            this.UserRepository = userRepository;
            this.Validator = validator;
            this.CurrentAddress = new Address();
        }

        public async Task InitializeAsync()
        {
            this.CurrentUser = await this.UserRepository.GetById(this.CurrentId);

            this.CurrentAddress = this.CurrentUser != null
                ? new Address(this.CurrentUser.Country, this.CurrentUser.City, this.CurrentUser.Street, this.CurrentUser.StreetNumber)
                : new Address();

            this.StateChanged?.Invoke();
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
            this.CurrentId = userId;

            this.CurrentUser = await this.UserRepository.GetById(userId);

            if (this.CurrentUser != null)
            {
                await this.UserRepository.GetById(this.CurrentUser.Id);

                this.CurrentAddress = new Address(
                    this.CurrentUser.Country,
                    this.CurrentUser.City,
                    this.CurrentUser.Street,
                    this.CurrentUser.StreetNumber);
            }
            else
            {
                this.CurrentAddress = new Address();
            }

            this.StateChanged?.Invoke();
        }

        public void OnFieldChange(string fieldName, string newValue)
        {
            typeof(Address).GetProperty(fieldName)?.SetValue(this.CurrentAddress, newValue);

            if (this.ValidationErrors.Remove(fieldName))
            {
                this.StateChanged?.Invoke();
            }
        }

        public void OpenMap()
        {
            this.IsMapVisible = true;
            this.StateChanged?.Invoke();
        }

        public void CloseMap()
        {
            this.IsMapVisible = false;
            this.StateChanged?.Invoke();
        }

        public async Task ConfirmMapLocationAsync(double latitude, double longitude)
        {
            Address resolved = await this.MapService.GetAddressFromMapAsync(latitude, longitude);

            if (resolved != null)
            {
                this.CurrentAddress = resolved;
                this.IsMapVisible = false;
                this.StateChanged?.Invoke();

                await this.SaveAddressIfRequestedAsync();
            }
        }

        public async Task SubmitDelivery()
        {
            this.ValidationErrors = this.Validator.Validate(this.CurrentAddress);
            this.StateChanged?.Invoke();

            if (this.ValidationErrors.Count == 0)
            {
                await this.SaveAddressIfRequestedAsync();
                this.OnNavigateToPayment?.Invoke();
            }
        }

        public void OnSaveAddressChanged(bool isChecked)
        {
            this.IsSaveAddress = isChecked;

            if (isChecked && this.CurrentUser != null)
            {
                _ = this.SaveAddressIfRequestedAsync();
            }
        }

        private async Task SaveAddressIfRequestedAsync()
        {
            if (this.IsSaveAddress && this.CurrentUser != null)
            {
                await this.UserRepository.SaveAddress(this.CurrentUser.Id, this.CurrentAddress);

                this.CurrentUser = await this.UserRepository.GetById(this.CurrentUser.Id);
                this.CurrentAddress = new Address(
                    this.CurrentUser.Country,
                    this.CurrentUser.City,
                    this.CurrentUser.Street,
                    this.CurrentUser.StreetNumber);

                this.StateChanged?.Invoke();
            }
        }
    }
}
