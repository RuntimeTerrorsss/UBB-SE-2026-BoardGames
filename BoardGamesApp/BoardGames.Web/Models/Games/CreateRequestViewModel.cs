using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BoardGames.Shared.DTO;

namespace BoardGames.Web.Models.Games
{
    public class CreateRequestViewModel
    {
        [Required(ErrorMessage = "Please select a game.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a game.")]
        public int GameId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public IReadOnlyList<GameDTO> AvailableGames { get; init; } = new List<GameDTO>();

        public string? ErrorMessage { get; init; }
    }
}
