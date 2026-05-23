using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGames.Data.Models
{
    [Table("rentals")]
    public class Rental
    {
        public Rental() { }

        public Rental(int id, int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate, decimal? totalPrice = null)
        {
            Id = id;
            GameId = gameId;
            ClientId = clientId;
            OwnerId = ownerId;
            StartDate = startDate;
            EndDate = endDate;
            TotalPrice = totalPrice;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("total_price")]
        public decimal? TotalPrice { get; set; }


        [Column("game_id")]
        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game? Game { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public User? Client { get; set; }

        [Column("owner_id")]
        public int OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public User? Owner { get; set; } 


        public Payment? Payment { get; set; }

        public ICollection<RentalRequestMessage> Messages { get; set; } = new List<RentalRequestMessage>();
    }
}