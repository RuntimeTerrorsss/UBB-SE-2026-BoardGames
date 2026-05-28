// <copyright file="GameDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class GameDTO
    {
        public int Id { get; set; }

        public int GameId
        {
            get => this.Id;
            set => this.Id = value;
        }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string City { get; set; } = string.Empty;

        public int MinimumPlayerNumber { get; set; }

        public int MaximumPlayerNumber { get; set; }

        public string Description { get; set; } = string.Empty;

        public byte[]? Image { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public UserDTO? Owner { get; set; }
    }
}
