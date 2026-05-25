using System;
using BoardGames.Shared.DTO;

namespace BoardGames.Desktop.Services
{
    public interface ISessionContext
    {
        Guid AccountId { get; }
        string Username { get; }
        string DisplayName { get; }
        string Email { get; }
        string PhoneNumber { get; }
        string Role { get; }
        string Country { get; }
        string City { get; }
        string StreetName { get; }
        string StreetNumber { get; }
        bool IsLoggedIn { get; }

        void Populate(AccountProfileDataTransferObject profile);
        void Clear();
    }

    public interface ICurrentUserContext
    {
        Guid CurrentUserId { get; }
    }

    public sealed class SessionContext : ISessionContext, ICurrentUserContext
    {
        public Guid AccountId { get; private set; } = Guid.Empty;
        public string Username { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Role { get; private set; } = AppRoles.StandardUser;
        public string Country { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string StreetName { get; private set; } = string.Empty;
        public string StreetNumber { get; private set; } = string.Empty;

        public bool IsLoggedIn => AccountId != Guid.Empty;

        public Guid CurrentUserId => AccountId;

        public void Populate(AccountProfileDataTransferObject profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            AccountId = profile.Id;
            Username = profile.Username ?? string.Empty;
            DisplayName = profile.DisplayName ?? string.Empty;
            Email = profile.Email ?? string.Empty;
            PhoneNumber = profile.PhoneNumber ?? string.Empty;
            Role = profile.Role?.Name ?? AppRoles.StandardUser;
            Country = profile.Country ?? string.Empty;
            City = profile.City ?? string.Empty;
            StreetName = profile.StreetName ?? string.Empty;
            StreetNumber = profile.StreetNumber ?? string.Empty;
        }

        public void Clear()
        {
            AccountId = Guid.Empty;
            Username = string.Empty;
            DisplayName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Role = AppRoles.StandardUser;
            Country = string.Empty;
            City = string.Empty;
            StreetName = string.Empty;
            StreetNumber = string.Empty;
        }
    }

    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly ISessionContext sessionContext;

        public CurrentUserContext(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public Guid CurrentUserId => sessionContext.AccountId;
    }
}
