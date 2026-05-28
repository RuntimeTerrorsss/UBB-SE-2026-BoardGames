using System;

namespace BoardGames.Shared.DTO
{
    public class GameSummaryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string City { get; set; } = string.Empty;

        public int MinimumPlayerNumber { get; set; }

        public int MaximumPlayerNumber { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string OwnerDisplayName { get; set; } = string.Empty;

        public Guid OwnerAccountId { get; set; }

        public bool IsActive { get; set; }
    }
}
