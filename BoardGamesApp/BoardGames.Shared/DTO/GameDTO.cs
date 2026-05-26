using System;

namespace BoardGames.Shared.DTO
{
    public class GameDTO
    {
        public int Id { get; set; }

        public UserDTO? Owner { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int MinimumPlayerNumber { get; set; }

        public int MaximumPlayerNumber { get; set; }

        public string Description { get; set; } = string.Empty;

        public byte[] Image { get; set; } = Array.Empty<byte>();

        public bool IsActive { get; set; }
    }
}
