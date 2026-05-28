// <copyright file="GameDtoMapperTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Infrastructure;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class GameDtoMapperTests
    {
        [Fact]
        public void FromSummary_MapsCoreFields()
        {
            var ownerId = Guid.NewGuid();
            var summary = new GameSummaryDTO
            {
                Id = 7,
                Name = "Catan",
                Price = 25m,
                City = "Cluj",
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                ImageUrl = "/img/catan.png",
                OwnerAccountId = ownerId,
                OwnerDisplayName = "Owner One",
                IsActive = true,
            };

            GameDTO mapped = GameDtoMapper.FromSummary(summary);

            Assert.Equal(7, mapped.Id);
            Assert.Equal(7, mapped.GameId);
            Assert.Equal("Catan", mapped.Name);
            Assert.Equal("Cluj", mapped.City);
            Assert.Equal(ownerId, mapped.Owner?.Id);
            Assert.Equal("Owner One", mapped.Owner?.DisplayName);
        }

        [Fact]
        public void ToSummary_RoundTripsIdentifier()
        {
            var game = new GameDTO
            {
                Id = 12,
                Name = "Ticket to Ride",
                Price = 30m,
                City = "Bucharest",
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 5,
                IsActive = false,
                Owner = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Alex" },
            };

            GameSummaryDTO summary = GameDtoMapper.ToSummary(game);

            Assert.Equal(12, summary.Id);
            Assert.Equal("Ticket to Ride", summary.Name);
            Assert.False(summary.IsActive);
        }
    }
}
