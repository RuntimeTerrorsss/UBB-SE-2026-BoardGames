// <copyright file="ViewModelAdapterTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Models.Dashboard;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class ViewModelAdapterTests
    {
        [Fact]
        public void ToProfileViewModel_MapsAllFields()
        {
            var profile = new AccountProfileDTO
            {
                Username = "jdoe",
                DisplayName = "John Doe",
                Email = "jdoe@example.com",
                PhoneNumber = "555-1234",
                Country = "Romania",
                City = "Cluj",
                StreetName = "Main St",
                StreetNumber = "42",
                AvatarUrl = "https://example.com/avatar.png",
            };

            var result = ViewModelAdapter.ToProfileViewModel(profile);

            Assert.Equal("jdoe", result.Username);
            Assert.Equal("John Doe", result.DisplayName);
            Assert.Equal("jdoe@example.com", result.Email);
            Assert.Equal("555-1234", result.PhoneNumber);
            Assert.Equal("Romania", result.Country);
            Assert.Equal("Cluj", result.City);
            Assert.Equal("Main St", result.StreetName);
            Assert.Equal("42", result.StreetNumber);
            Assert.Equal("https://example.com/avatar.png", result.AvatarUrl);
        }

        [Fact]
        public void ToProfileViewModel_NullFields_DefaultToEmpty()
        {
            var profile = new AccountProfileDTO
            {
                DisplayName = null,
                Email = null,
                AvatarUrl = null,
            };

            var result = ViewModelAdapter.ToProfileViewModel(profile);

            Assert.Equal(string.Empty, result.DisplayName);
            Assert.Equal(string.Empty, result.Email);
            Assert.Equal(string.Empty, result.AvatarUrl);
        }

        [Fact]
        public void ToAccountProfileDTO_MapsAllFields()
        {
            var model = new BoardGames.Web.Models.Account.ProfileViewModel
            {
                DisplayName = "Jane Doe",
                Email = "jane@example.com",
                PhoneNumber = "555-9999",
                Country = "Romania",
                City = "Bucharest",
                StreetName = "Oak Ave",
                StreetNumber = "7",
            };

            var result = ViewModelAdapter.ToAccountProfileDTO(model);

            Assert.Equal("Jane Doe", result.DisplayName);
            Assert.Equal("jane@example.com", result.Email);
            Assert.Equal("555-9999", result.PhoneNumber);
            Assert.Equal("Romania", result.Country);
            Assert.Equal("Bucharest", result.City);
            Assert.Equal("Oak Ave", result.StreetName);
            Assert.Equal("7", result.StreetNumber);
        }

        [Fact]
        public void ToDashboardViewModel_AssemblesCorrectly()
        {
            var rentals = new List<RentalDTO>
            {
                new RentalDTO { Id = 1 },
                new RentalDTO { Id = 2 },
            };
            var requests = new List<RequestDTO> { new RequestDTO { Id = 10 } };
            var payments = new List<PaymentDTO> { new PaymentDTO { PaymentId = 100 } };

            var result = ViewModelAdapter.ToDashboardViewModel(rentals, requests, payments);

            Assert.IsType<DashboardViewModel>(result);
            Assert.Equal(2, result.UpcomingRentals.Count);
            Assert.Single(result.OpenRequests);
            Assert.Single(result.RecentPayments);
        }
    }
}
