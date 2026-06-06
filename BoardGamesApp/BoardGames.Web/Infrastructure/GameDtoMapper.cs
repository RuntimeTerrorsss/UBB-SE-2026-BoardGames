// <copyright file="GameDtoMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Infrastructure
{
    public static class GameDtoMapper
    {
        public static GameDTO FromSummary(GameSummaryDTO summary) => new()
        {
            Id = summary.Id,
            Name = summary.Name,
            Price = summary.Price,
            City = summary.City,
            MinimumPlayerNumber = summary.MinimumPlayerNumber,
            MaximumPlayerNumber = summary.MaximumPlayerNumber,
            ImageUrl = summary.ImageUrl,
            IsActive = summary.IsActive,
            Owner = new UserDTO
            {
                Id = summary.OwnerAccountId,
                DisplayName = summary.OwnerDisplayName,
            },
        };

        public static GameDTO FromDetail(GameDetailDTO detail) => new()
        {
            Id = detail.Id,
            Name = detail.Name,
            Price = detail.Price,
            City = detail.City,
            MinimumPlayerNumber = detail.MinimumPlayerNumber,
            MaximumPlayerNumber = detail.MaximumPlayerNumber,
            Description = detail.Description,
            ImageUrl = detail.ImageUrl,
            IsActive = detail.IsActive,
            Owner = detail.Owner ?? new UserDTO
            {
                Id = detail.OwnerAccountId,
                DisplayName = detail.OwnerDisplayName,
            },
        };

        public static GameSummaryDTO ToSummary(GameDTO game) => new()
        {
            Id = game.Id,
            Name = game.Name,
            Price = game.Price,
            City = game.City,
            MinimumPlayerNumber = game.MinimumPlayerNumber,
            MaximumPlayerNumber = game.MaximumPlayerNumber,
            ImageUrl = game.ImageUrl,
            IsActive = game.IsActive,
            OwnerAccountId = game.Owner?.Id ?? Guid.Empty,
            OwnerDisplayName = game.Owner?.DisplayName ?? string.Empty,
        };
    }
}
