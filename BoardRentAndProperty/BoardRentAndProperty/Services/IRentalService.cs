using System;
using System.Collections.Immutable;
using BoardRentAndProperty.DataTransferObjects;
namespace BoardRentAndProperty.Services
{
    public interface IRentalService
    {
        ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId);
        ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId);
        bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate);
        void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);
    }
}
