using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
namespace BoardRentAndProperty.Repositories
{
    public interface IRentalRepository : IRepository<Rental>
    {
        void AddConfirmed(Rental confirmedRental);
        ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId);
        ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId);
        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}
