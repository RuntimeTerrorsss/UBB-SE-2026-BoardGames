using System;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid CurrentUserId { get; set; }
    }

    internal sealed class FakeSessionContext : ISessionContext
    {
        public Guid AccountId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLoggedIn { get; set; }
        public int PopulateCallCount { get; private set; }
        public int ClearCallCount { get; private set; }

        public void Populate(AccountProfileDataTransferObject profile)
        {
            PopulateCallCount++;
            AccountId = profile.Id;
            Username = profile.Username;
            DisplayName = profile.DisplayName;
            Email = profile.Email;
            PhoneNumber = profile.PhoneNumber;
            Country = profile.Country;
            City = profile.City;
            StreetName = profile.StreetName;
            StreetNumber = profile.StreetNumber;
            Role = profile.Role.Name;
            IsLoggedIn = true;
        }

        public void Clear()
        {
            ClearCallCount++;
            AccountId = Guid.Empty;
            Username = string.Empty;
            DisplayName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Country = string.Empty;
            City = string.Empty;
            StreetName = string.Empty;
            StreetNumber = string.Empty;
            Role = string.Empty;
            IsLoggedIn = false;
        }
    }
}
