namespace BoardGames.Data.Models
{
    public class FailedLoginAttempt
    {
        public Guid AccountId { get; set; }
        public User Account { get; set; } = default!;
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
