/*
using System;
using System.Collections.Immutable;
using BoardGames.Data.Models;

namespace BoardGames.Data.Repositories
{
    public interface IRentalRepository2
    {
        ImmutableList<Rental> GetAll();
        void Add(Rental rental);
        Rental Delete(int id);
        void Update(int id, Rental updated);
        Rental Get(int id);
        void AddConfirmed(Rental confirmedRental);
        ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId);
        ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId);
        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}
*/
