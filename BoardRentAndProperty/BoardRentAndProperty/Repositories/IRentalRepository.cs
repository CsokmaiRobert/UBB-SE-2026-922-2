using System.Collections.Immutable;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Services
{
    public interface IRentalRepository : IRepository<Rental>
    {
        void AddConfirmed(Rental confirmedRental);

        ImmutableList<Rental> GetRentalsByOwner(int ownerUserId);

        ImmutableList<Rental> GetRentalsByRenter(int renterUserId);

        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}