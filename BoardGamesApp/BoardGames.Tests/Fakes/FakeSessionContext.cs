// <copyright file="FakeSessionContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeSessionContext : ISessionContext
    {
        public Guid AccountId { get; set; }

        public int? PamUserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public bool IsSuspended { get; set; }

        public bool IsLocked { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string StreetName { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;

        public string Role { get; set; } = AppRoles.StandardUser;

        public bool IsLoggedIn { get; set; }

        public int PopulateCallCount { get; private set; }

        public int ClearCallCount { get; private set; }

        public void Populate(AccountProfileDTO profile)
        {
            this.PopulateCallCount++;
            this.AccountId = profile.Id;
            this.PamUserId = profile.PamUserId;
            this.Username = profile.Username ?? string.Empty;
            this.DisplayName = profile.DisplayName ?? string.Empty;
            this.Email = profile.Email ?? string.Empty;
            this.AvatarUrl = profile.AvatarUrl ?? string.Empty;
            this.IsSuspended = profile.IsSuspended;
            this.IsLocked = profile.IsLocked;
            this.PhoneNumber = profile.PhoneNumber ?? string.Empty;
            this.Country = profile.Country ?? string.Empty;
            this.City = profile.City ?? string.Empty;
            this.StreetName = profile.StreetName ?? string.Empty;
            this.StreetNumber = profile.StreetNumber ?? string.Empty;
            this.Role = profile.Role?.Name ?? AppRoles.StandardUser;
            this.IsLoggedIn = true;
        }

        public void Clear()
        {
            this.ClearCallCount++;
            this.AccountId = Guid.Empty;
            this.PamUserId = null;
            this.Username = string.Empty;
            this.DisplayName = string.Empty;
            this.Email = string.Empty;
            this.AvatarUrl = string.Empty;
            this.IsSuspended = false;
            this.IsLocked = false;
            this.PhoneNumber = string.Empty;
            this.Country = string.Empty;
            this.City = string.Empty;
            this.StreetName = string.Empty;
            this.StreetNumber = string.Empty;
            this.Role = AppRoles.StandardUser;
            this.IsLoggedIn = false;
        }
    }
}
