// <copyright file="Game.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGames.Data.Models
{
    [Table("games")]
    public class Game
    {
        public Game() { }

        public Game(int id, string name, decimal pricePerDay, int minimumPlayerNumber, int maximumPlayerNumber, string description, int ownerId, byte[]? image, bool isActive)
        {
            this.Id = id;
            this.Name = name;
            this.PricePerDay = pricePerDay;
            this.MinimumPlayerNumber = minimumPlayerNumber;
            this.MaximumPlayerNumber = maximumPlayerNumber;
            this.Description = description;
            this.OwnerId = ownerId;
            this.Image = image;
            this.IsActive = isActive;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("price")]
        public decimal PricePerDay { get; set; }

        [Column("minimum_player_number")]
        public int MinimumPlayerNumber { get; set; }

        [Column("maximum_player_number")]
        public int MaximumPlayerNumber { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("image")]
        public byte[]? Image { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("owner_id")]
        public int OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public User? Owner { get; set; }

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
