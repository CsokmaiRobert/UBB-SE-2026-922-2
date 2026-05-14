using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Models;

namespace BoardRentAndProperty.Api.Repositories
{
    public interface IRentalRepository
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
