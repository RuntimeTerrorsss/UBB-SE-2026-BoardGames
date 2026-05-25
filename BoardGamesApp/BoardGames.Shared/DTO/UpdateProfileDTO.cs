// <copyright file="UpdateProfileDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class UpdateProfileDTO
    {
        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string StreetName { get; set; }

        public string StreetNumber { get; set; }
    }
}
