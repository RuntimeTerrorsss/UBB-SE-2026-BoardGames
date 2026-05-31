using System;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public class GameMapper
    {
        private readonly UserMapper ownerMapper;

        public GameMapper(UserMapper ownerMapper)
        {
            this.ownerMapper = ownerMapper;
        }

        private string GetImageUrl(Game game)
        {
            if (game.Image != null && game.Image.Length > 0)
            {
                return $"/api/games/{game.Id}/image";
            }
            return GameImageMapper.GetImageUrl(game.Name);
        }

        public GameSummaryDTO ToSummaryDTO(Game game)
        {
            return new GameSummaryDTO
            {
                Id = game.Id,
                Name = game.Name,
                Price = game.PricePerDay,
                City = game.Owner?.City ?? string.Empty,
                MinimumPlayerNumber = game.MinimumPlayerNumber,
                MaximumPlayerNumber = game.MaximumPlayerNumber,
                ImageUrl = GetImageUrl(game),
                OwnerDisplayName = game.Owner?.DisplayName ?? string.Empty,
                OwnerAccountId = game.Owner?.Id ?? Guid.Empty,
                IsActive = game.IsActive
            };
        }

        public GameDetailDTO ToDetailDTO(Game game)
        {
            return new GameDetailDTO
            {
                Id = game.Id,
                Name = game.Name,
                Price = game.PricePerDay,
                City = game.Owner?.City ?? string.Empty,
                MinimumPlayerNumber = game.MinimumPlayerNumber,
                MaximumPlayerNumber = game.MaximumPlayerNumber,
                ImageUrl = GetImageUrl(game),
                OwnerDisplayName = game.Owner?.DisplayName ?? string.Empty,
                OwnerAccountId = game.Owner?.Id ?? Guid.Empty,
                IsActive = game.IsActive,
                Description = game.Description,
                Owner = ownerMapper.ToDTO(game.Owner)
            };
        }

        public Game ToModel(GameCreateDTO dto, Guid ownerAccountId)
        {
            return new Game
            {
                Name = dto.Name,
                PricePerDay = dto.Price,
                MinimumPlayerNumber = dto.MinimumPlayerNumber,
                MaximumPlayerNumber = dto.MaximumPlayerNumber,
                Description = dto.Description,
                Image = dto.Image,
                IsActive = true, // By default games are active when created
                Owner = new User { Id = ownerAccountId } // Repository resolves this via ResolveUser
            };
        }

        public void ApplyUpdate(Game existing, GameUpdateDTO dto)
        {
            existing.Name = dto.Name;
            existing.PricePerDay = dto.Price;
            existing.MinimumPlayerNumber = dto.MinimumPlayerNumber;
            existing.MaximumPlayerNumber = dto.MaximumPlayerNumber;
            existing.Description = dto.Description;
            existing.Image = dto.Image;
            existing.IsActive = dto.IsActive;
        }
    }
}
