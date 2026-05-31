//// <copyright file="FakeSessionContext.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using BoardGames.Desktop.Services;
//using BoardGames.Shared.DTO;

//namespace BoardGames.Tests.Fakes
//{
//    internal sealed class FakeSessionContext : ISessionContext
//    {
//        public Guid AccountId { get; set; }

//        public int? PamUserId { get; set; }

//        public string Username { get; set; } = string.Empty;

//        public string DisplayName { get; set; } = string.Empty;

//        public string Email { get; set; } = string.Empty;

//        public string Role { get; set; } = AppRoles.StandardUser;

//        public string AvatarUrl { get; set; } = string.Empty;

//        public bool IsSuspended { get; set; }

//        public bool IsLocked { get; set; }

//        public string PhoneNumber { get; set; } = string.Empty;

//        public string Country { get; set; } = string.Empty;

//        public string City { get; set; } = string.Empty;

//        public string StreetName { get; set; } = string.Empty;

//        public string StreetNumber { get; set; } = string.Empty;

//        public bool IsLoggedIn { get; set; }

//        public int PopulateCallCount { get; private set; }

//        public int ClearCallCount { get; private set; }

//        public void Populate(AccountProfileDTO accountProfile)
//        {
//            this.PopulateCallCount++;
//            this.AccountId = accountProfile.Id;
//            this.PamUserId = accountProfile.PamUserId;
//            this.Username = accountProfile.Username ?? string.Empty;
//            this.DisplayName = accountProfile.DisplayName ?? string.Empty;
//            this.Email = accountProfile.Email ?? string.Empty;
//            this.Role = accountProfile.Role?.Name ?? AppRoles.StandardUser;
//            this.AvatarUrl = accountProfile.AvatarUrl ?? string.Empty;
//            this.IsSuspended = accountProfile.IsSuspended;
//            this.IsLocked = accountProfile.IsLocked;
//            this.PhoneNumber = accountProfile.PhoneNumber ?? string.Empty;
//            this.Country = accountProfile.Country ?? string.Empty;
//            this.City = accountProfile.City ?? string.Empty;
//            this.StreetName = accountProfile.StreetName ?? string.Empty;
//            this.StreetNumber = accountProfile.StreetNumber ?? string.Empty;
//            this.IsLoggedIn = true;
//        }

//        public void Clear()
//        {
//            this.ClearCallCount++;
//            this.AccountId = Guid.Empty;
//            this.PamUserId = null;
//            this.Username = string.Empty;
//            this.DisplayName = string.Empty;
//            this.Email = string.Empty;
//            this.Role = AppRoles.StandardUser;
//            this.AvatarUrl = string.Empty;
//            this.IsSuspended = false;
//            this.IsLocked = false;
//            this.PhoneNumber = string.Empty;
//            this.Country = string.Empty;
//            this.City = string.Empty;
//            this.StreetName = string.Empty;
//            this.StreetNumber = string.Empty;
//            this.IsLoggedIn = false;
//        }
//    }
//}
