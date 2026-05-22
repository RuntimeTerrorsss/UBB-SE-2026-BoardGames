using System.Collections.Generic;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardGames.Web.Models.Rentals
{
    public class MyRequestsViewModel
    {
        public IReadOnlyList<RequestDTO> Requests { get; init; } = new List<RequestDTO>();

        public string? ErrorMessage { get; init; }
    }
}
