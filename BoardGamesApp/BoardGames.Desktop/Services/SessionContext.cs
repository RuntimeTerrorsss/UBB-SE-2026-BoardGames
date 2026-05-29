// <copyright file="SessionContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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

        public bool IsLoggedIn => this.AccountId != Guid.Empty;

        public void Populate(AccountProfileDTO accountProfile)
        {
            ArgumentNullException.ThrowIfNull(accountProfile);

            this.AccountId = accountProfile.Id;
            this.PamUserId = accountProfile.PamUserId;
            this.Username = accountProfile.Username ?? string.Empty;
            this.DisplayName = accountProfile.DisplayName ?? string.Empty;
            this.Email = accountProfile.Email ?? string.Empty;
            this.Role = accountProfile.Role?.Name ?? AppRoles.StandardUser;
            this.AvatarUrl = accountProfile.AvatarUrl ?? string.Empty;
            this.IsSuspended = accountProfile.IsSuspended;
            this.IsLocked = accountProfile.IsLocked;
            this.PhoneNumber = accountProfile.PhoneNumber ?? string.Empty;
            this.Country = accountProfile.Country ?? string.Empty;
            this.City = accountProfile.City ?? string.Empty;
            this.StreetName = accountProfile.StreetName ?? string.Empty;
            this.StreetNumber = accountProfile.StreetNumber ?? string.Empty;
        }

        public void Clear()
        {
            this.AccountId = Guid.Empty;
            this.PamUserId = null;
            this.Username = string.Empty;
            this.DisplayName = string.Empty;
            this.Email = string.Empty;
            this.Role = AppRoles.StandardUser;
            this.AvatarUrl = string.Empty;
            this.IsSuspended = false;
            this.IsLocked = false;
            this.PhoneNumber = string.Empty;
            this.Country = string.Empty;
            this.City = string.Empty;
            this.StreetName = string.Empty;
            this.StreetNumber = string.Empty;
        }
    }
}
