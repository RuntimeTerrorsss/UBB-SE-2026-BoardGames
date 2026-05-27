// <copyright file="UserDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        public int? PamUserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }
}
