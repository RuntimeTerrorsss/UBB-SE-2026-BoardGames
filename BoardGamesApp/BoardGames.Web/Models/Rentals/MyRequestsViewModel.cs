using System.Collections.Generic;
using BoardGames.Shared.DTO;

namespace BoardGames.Web.Models.Rentals
{
    public class MyRequestsViewModel
    {
        public IReadOnlyList<RequestDTO> Requests { get; init; } = new List<RequestDTO>();

        public string? ErrorMessage { get; init; }
    }
}
