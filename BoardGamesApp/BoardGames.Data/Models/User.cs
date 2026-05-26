using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGames.Data.Models
{
    [Table("user_account")]
    public class User
    {
        public User() { }

        public User(Guid id, int pamUserId, string username, string displayName, string email, string passwordHash, string country, string city, string? streetName, string? streetNumber, string? avatarUrl, decimal balance)
        {
            this.Id = id;
            this.PamUserId = pamUserId;
            this.Username = username;
            this.DisplayName = displayName;
            this.Email = email;
            this.PasswordHash = passwordHash;
            this.Country = country;
            this.City = city;
            this.StreetName = streetName;
            this.StreetNumber = streetNumber;
            this.AvatarUrl = avatarUrl;
            this.Balance = balance;
        }

        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("pam_user_id")]
        public int PamUserId { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
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

        [Column("country")]
        public string Country { get; set; } = string.Empty;

        [Column("city")]
        public string City { get; set; } = string.Empty;

        [Column("street_name")]
        public string? StreetName { get; set; }

        [Column("street_number")]
        public string? StreetNumber { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();

        [InverseProperty("Owner")]
        public ICollection<Game> OwnedGames { get; set; } = new List<Game>();

        [InverseProperty("Client")]
        public ICollection<Rental> RentalsAsClient { get; set; } = new List<Rental>();

        [InverseProperty("Owner")]
        public ICollection<Rental> RentalsAsOwner { get; set; } = new List<Rental>();

        public ICollection<ConversationParticipant> Conversations { get; set; } = new List<ConversationParticipant>();
    }
}
