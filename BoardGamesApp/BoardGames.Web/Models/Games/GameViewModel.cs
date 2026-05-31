using System.ComponentModel.DataAnnotations;

namespace BoardGames.Web.Models.Games
{
    public class GameViewModel
    {
        public int GameId { get; set; }

        [Required(ErrorMessage = "Please enter a name for the game.")]
        [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
        [Display(Name = "Game Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Cover Image URL")]
        public string Image { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a price.")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Minimum Players")]
        [Range(1, 100, ErrorMessage = "Must be at least 1 player.")]
        public int MinimumPlayerNumber { get; set; }

        [Required]
        [Display(Name = "Maximum Players")]
        [Range(1, 100, ErrorMessage = "Must be at least 1 player.")]
        public int MaximumPlayerNumber { get; set; }
    }
}
