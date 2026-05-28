namespace BoardGames.Desktop.Services
{
    using System;
    using BoardGames.Shared.DTO;

    public interface ISessionContext
    {
        Guid AccountId { get; }

        int? PamUserId { get; }

        string Username { get; }

        string DisplayName { get; }

        string Email { get; }

        string Role { get; }

        string AvatarUrl { get; }

        bool IsSuspended { get; }

        bool IsLocked { get; }

        string PhoneNumber { get; }

        string Country { get; }

        string City { get; }

        string StreetName { get; }

        string StreetNumber { get; }

        bool IsLoggedIn { get; }

        void Populate(AccountProfileDTO accountProfile);

        void Clear();
    }
}