// <copyright file="AccountProfileDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class AccountProfileDTO
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string AvatarUrl { get; set; }

        public RoleDTO Role { get; set; }

        public bool IsSuspended { get; set; }

        public bool IsLocked { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string StreetName { get; set; }

        public string StreetNumber { get; set; }
    }
}
