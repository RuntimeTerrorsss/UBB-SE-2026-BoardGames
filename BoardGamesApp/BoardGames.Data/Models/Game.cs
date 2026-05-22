using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("games")]
public class Game
{
    public Game(string name, decimal pricePerDay, int minimumPlayerNumber, int maximumPlayerNumber, string description, int ownerId)
    {
        Name = name;
        PricePerDay = pricePerDay;
        MinimumPlayerNumber = minimumPlayerNumber;
        MaximumPlayerNumber = maximumPlayerNumber;
        Description = description;
        OwnerId = ownerId;
        IsActive = true;
    }

    public Game(decimal p1, int p2, int p3, string p4, int p5, int p6, int p7, string p8, int p9) { Id = p5; Name = p4; PricePerDay = p1; MinimumPlayerNumber = p2; MaximumPlayerNumber = p3; Description = p8; OwnerId = p9; } public Game() {
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]

    public string Name { get; set; } = string.Empty;

    [Column("price")]

    public decimal PricePerDay { get; set; }

    [Column("maximum_player_number")]

    public int MaximumPlayerNumber { get; set; }

    [Column("minimum_player_number")]

    public int MinimumPlayerNumber { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("image")]

    public byte[]? Image { get; set; }

    [Column("is_active")]

    public bool IsActive { get; set; }

    [Column("owner_id")]

    public int OwnerId { get; set; }

    [ForeignKey("OwnerId")]

    public User? Owner { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
