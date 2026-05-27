// <copyright file="AccountProfileDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class AccountProfileDTO
    {
        public Guid Id { get; set; }

        public int? PamUserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public RoleDTO? Role { get; set; }

        public bool IsSuspended { get; set; }

        public bool IsLocked { get; set; }

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string StreetName { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;
    }
}
