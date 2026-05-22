using System.ComponentModel.DataAnnotations;

namespace BoardGames.Web.Models.Chats
{
    public class ConversationViewModel
    {
        public int ConversationId { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Other User Name")]
        [StringLength(50)]
        public string OtherUserName { get; set; } = string.Empty;

        [Display(Name = "Last Message")]
        public string LastMessageContent { get; set; } = string.Empty;
    }
}
