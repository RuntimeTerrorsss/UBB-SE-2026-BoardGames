using System;
using BoardGames.Shared.DTO;

namespace BoardGames.Desktop.Services
{
    public sealed class SessionContext : ISessionContext
    {
        public Guid AccountId { get; private set; }

        public int? PamUserId { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string DisplayName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Role { get; private set; } = AppRoles.StandardUser;

        public string AvatarUrl { get; private set; } = string.Empty;

        public bool IsSuspended { get; private set; }

        public bool IsLocked { get; private set; }

        public string PhoneNumber { get; private set; } = string.Empty;

        public string Country { get; private set; } = string.Empty;

        public string City { get; private set; } = string.Empty;

        public string StreetName { get; private set; } = string.Empty;

        public string StreetNumber { get; private set; } = string.Empty;

        public bool IsLoggedIn => AccountId != Guid.Empty;

        public void Populate(AccountProfileDTO accountProfile)
        {
            ArgumentNullException.ThrowIfNull(accountProfile);

            AccountId = accountProfile.Id;
            PamUserId = accountProfile.PamUserId;
            Username = accountProfile.Username ?? string.Empty;
            DisplayName = accountProfile.DisplayName ?? string.Empty;
            Email = accountProfile.Email ?? string.Empty;
            Role = accountProfile.Role?.Name ?? AppRoles.StandardUser;
            AvatarUrl = accountProfile.AvatarUrl ?? string.Empty;
            IsSuspended = accountProfile.IsSuspended;
            IsLocked = accountProfile.IsLocked;
            PhoneNumber = accountProfile.PhoneNumber ?? string.Empty;
            Country = accountProfile.Country ?? string.Empty;
            City = accountProfile.City ?? string.Empty;
            StreetName = accountProfile.StreetName ?? string.Empty;
            StreetNumber = accountProfile.StreetNumber ?? string.Empty;
        }

        public void Clear()
        {
            AccountId = Guid.Empty;
            PamUserId = null;
            Username = string.Empty;
            DisplayName = string.Empty;
            Email = string.Empty;
            Role = AppRoles.StandardUser;
            AvatarUrl = string.Empty;
            IsSuspended = false;
            IsLocked = false;
            PhoneNumber = string.Empty;
            Country = string.Empty;
            City = string.Empty;
            StreetName = string.Empty;
            StreetNumber = string.Empty;
        }
    }
}
