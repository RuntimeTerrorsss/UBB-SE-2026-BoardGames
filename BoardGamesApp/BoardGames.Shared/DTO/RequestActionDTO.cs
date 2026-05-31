using System;

namespace BoardGames.Shared.DTO
{
    public class RequestActionDataTransferObject
    {
        public Guid AccountId { get; set; }

        public string Reason { get; set; }
    }
}
