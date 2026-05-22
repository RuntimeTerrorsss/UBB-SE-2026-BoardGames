using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class User
{
    public User(string username, string displayName, string email, string passwordHash, string city, string country)
    {
        Username = username;
        DisplayName = displayName;
        Email = email;
        PasswordHash = passwordHash;
        City = city;
        Country = country;
        Balance = 0m;
        IsSuspended = false;
        CreatedAt = DateTime.UtcNow;
    }

    public User(int id, string username, string country, string city, string street, string streetNumber, string displayName, string avatarUrl, decimal balance) { Id = id; Username = username; Country = country; City = city; Street = street; StreetNumber = streetNumber; DisplayName = displayName; AvatarUrl = avatarUrl; Balance = balance; } 
    public User() 
    {
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("balance")]
    public decimal Balance { get; set; } = 0m;

    [Column("is_suspended")]
    public bool IsSuspended { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("street")]
    public string? Street { get; set; }

    [Column("street_number")]
    public string? StreetNumber { get; set; }

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("country")]
    public string Country { get; set; } = string.Empty;

    public ICollection<Game> OwnedGames { get; set; } = new List<Game>();
    public ICollection<Rental> RentalsAsClient { get; set; } = new List<Rental>();
    public ICollection<Rental> RentalsAsOwner { get; set; } = new List<Rental>();
    public ICollection<ConversationParticipant> Conversations { get; set; } = new List<ConversationParticipant>();
}
