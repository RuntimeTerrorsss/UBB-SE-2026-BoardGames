// <copyright file="IGameRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IGameRepository
    {
        ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId);

        void AddGame(Game game);

        Game DeleteGame(int id);

        void UpdateGame(int id, Game updated);

        Game GetGame(int id);
    }
}
