using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGames.Data.Models
{
    public class AccountRole
    {
        public Guid AccountId { get; set; }

        public User Account { get; set; } = default!;

        public Guid RoleId { get; set; }

        public Role Role { get; set; } = default!;
    }

    [Table("user_account_role")]
    public class UserAccountRole
    {
        [Column("user_account_id")]
        public int UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public User UserAccount { get; set; } = default!;

        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = default!;
    }
}
