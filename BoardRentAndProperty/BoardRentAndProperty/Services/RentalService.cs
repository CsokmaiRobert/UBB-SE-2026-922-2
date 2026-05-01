using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
namespace BoardRentAndProperty.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository rentalDataRepository;
        private readonly IGameRepository gameLookupRepository;
        private readonly IMapper<Rental, RentalDTO, int> rentalDtoMapper;
        private const int NewRentalId = 0;
        public RentalService(IRentalRepository rentalRepository, IGameRepository gameRepository, IMapper<Rental, RentalDTO, int> rentalMapper)
        {
            rentalDataRepository = rentalRepository;
            gameLookupRepository = gameRepository;
            rentalDtoMapper = rentalMapper;
        }
        public bool IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)
        {
            foreach (var r in rentalDataRepository.GetRentalsByGame(gameId))
            {
                if (startDate < r.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > r.StartDate.AddHours(-DomainConstants.RentalBufferHours))
                {
                    return false;
                }
            }
            return true;
        }
        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate))
            {
                throw new ArgumentException("Start date must be before end date and not in the past.");
            }
            var game = gameLookupRepository.Get(gameId);
            if (game.Owner?.Id != ownerAccountId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }
            if (!IsSlotAvailable(gameId, startDate, endDate))
            {
                throw new InvalidOperationException($"Selected dates fall within the mandatory {DomainConstants.RentalBufferHours}-hour buffer of another rental.");
            }
           var rental = new Rental(NewRentalId, new Game { Id = gameId }, new Account { Id = renterAccountId }, new Account { Id = ownerAccountId }, startDate, endDate);
            rentalDataRepository.AddConfirmed(rental);
        }
        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) =>
            rentalDataRepository.GetRentalsByRenter(renterAccountId).Select(r => rentalDtoMapper.ToDTO(r)).ToImmutableList();
        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) =>
            rentalDataRepository.GetRentalsByOwner(ownerAccountId).Select(r => rentalDtoMapper.ToDTO(r)).ToImmutableList();
    }
}
