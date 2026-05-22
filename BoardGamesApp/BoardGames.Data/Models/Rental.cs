using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("rentals")]
public class Rental
{
    public Rental(DateTime startDate, DateTime endDate, int gameId, int clientId, int ownerId, decimal? totalPrice = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        GameId = gameId;
        ClientId = clientId;
        OwnerId = ownerId;
        TotalPrice = totalPrice;
    }

    public Rental(int rentalId, int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate) { RentalId = rentalId; GameId = gameId; ClientId = clientId; OwnerId = ownerId; StartDate = startDate; EndDate = endDate; } public Rental() {
    }

    [Key]
    [Column("id")]
    public int RentalId { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("total_price")]
    public decimal? TotalPrice { get; set; }

    [Column("game_id")]
    public int GameId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("owner_id")]
    public int OwnerId { get; set; }

    [ForeignKey("GameId")]
    public Game? Game { get; set; }

    [ForeignKey("ClientId")]
    public User? Client { get; set; }

    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }

    public Payment? Payment { get; set; }

    public ICollection<RentalRequestMessage> Messages { get; set; } = new List<RentalRequestMessage>();
}
