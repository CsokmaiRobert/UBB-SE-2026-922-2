using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Repositories
{
    public interface IRentalRepository : IRepository<Rental>
    {
        void AddConfirmed(Rental confirmedRental);

        ImmutableList<Rental> GetRentalsByOwner(int ownerUserId);

        ImmutableList<Rental> GetRentalsByRenter(int renterUserId);

        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}