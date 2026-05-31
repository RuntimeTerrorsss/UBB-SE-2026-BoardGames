using System.ComponentModel.DataAnnotations;
using BoardGames.Shared.DTO;

namespace BoardGames.Web.Models.Search
{
    public class SearchFilterViewModel
    {
        [Display(Name = "Game name")]
        public string? Name { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Start date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Maximum price")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1 and 1000.")]
        public decimal? MaximumPrice { get; set; }

        [Display(Name = "Minimum players")]
        [Range(1, 20, ErrorMessage = "Players must be between 1 and 20.")]
        public int? MinimumPlayers { get; set; }

        [Display(Name = "Sort by")]
        public string? SortOption { get; set; }

        [Display(Name = "Page")]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 6;
        public int TotalPages { get; set; } = 1;

        public List<GameDTO> Results { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}