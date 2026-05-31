// <copyright file="GameCreateDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;

namespace BoardGames.Shared.DTO
{
    public class GameCreateDTO
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int MinimumPlayerNumber { get; set; }

        public int MaximumPlayerNumber { get; set; }

        public string Description { get; set; } = string.Empty;

        public byte[] Image { get; set; } = Array.Empty<byte>();

        // This acts as a placeholder for Task 7. 
        // Currently used by the API to identify the owner since session is not yet wired up.
        public Guid OwnerAccountId { get; set; }
    }
}
