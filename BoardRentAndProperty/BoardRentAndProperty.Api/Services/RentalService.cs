using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Constants;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public class RentalService : IRentalService
    {
        private const int NewRentalId = 0;

        private readonly IRentalRepository rentalDataRepository;
        private readonly IGameRepository gameLookupRepository;
        private readonly RentalMapper rentalDtoMapper;

        public RentalService(IRentalRepository rentalRepository, IGameRepository gameRepository, RentalMapper rentalMapper)
        {
            this.rentalDataRepository = rentalRepository;
            this.gameLookupRepository = gameRepository;
            this.rentalDtoMapper = rentalMapper;
        }

        public bool IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)
        {
            foreach (var rental in this.rentalDataRepository.GetRentalsByGame(gameId))
            {
                if (startDate < rental.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > rental.StartDate.AddHours(-DomainConstants.RentalBufferHours))
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

            var game = this.gameLookupRepository.Get(gameId);
            if (game.Owner?.Id != ownerAccountId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }

            if (!IsSlotAvailable(gameId, startDate, endDate))
            {
                throw new InvalidOperationException($"Selected dates fall within the mandatory {DomainConstants.RentalBufferHours}-hour buffer of another rental.");
            }

            var rental = new Rental(NewRentalId, new Game { Id = gameId }, new Account { Id = renterAccountId }, new Account { Id = ownerAccountId }, startDate, endDate);
            this.rentalDataRepository.AddConfirmed(rental);
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) =>
            this.rentalDataRepository.GetRentalsByRenter(renterAccountId).Select(r => this.rentalDtoMapper.ToDTO(r)!).ToImmutableList();

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) =>
            this.rentalDataRepository.GetRentalsByOwner(ownerAccountId).Select(r => this.rentalDtoMapper.ToDTO(r)!).ToImmutableList();
    }
}
