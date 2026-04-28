using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository rentalDataRepository;
        private readonly IGameRepository gameLookupRepository;
        private readonly IMapper<Rental, RentalDTO> rentalDtoMapper;
        private readonly IRequestRepository requestDataRepository;
        private readonly INotificationService notificationEventService;

        private const int NewRentalId = 0;

        public RentalService(
            IRentalRepository rentalRepository,
            IGameRepository gameRepository,
            IMapper<Rental, RentalDTO> rentalMapper,
            IRequestRepository requestRepository,
            INotificationService notificationService)
        {
            this.rentalDataRepository = rentalRepository;
            this.gameLookupRepository = gameRepository;
            this.rentalDtoMapper = rentalMapper;
            this.requestDataRepository = requestRepository;
            this.notificationEventService = notificationService;
        }

        public bool IsSlotAvailable(int gameId, DateTime proposedStartDate, DateTime proposedEndDate)
        {
            foreach (var existingRental in rentalDataRepository.GetRentalsByGame(gameId))
            {
                var bufferStart = existingRental.StartDate.AddHours(-DomainConstants.RentalBufferHours);
                var bufferEnd = existingRental.EndDate.AddHours(DomainConstants.RentalBufferHours);
                if (proposedStartDate < bufferEnd && proposedEndDate > bufferStart)
                {
                    return false;
                }
            }

            foreach (var existingRequest in requestDataRepository.GetRequestsByGame(gameId))
            {
                if (existingRequest.Status == RequestStatus.Cancelled)
                {
                    continue;
                }

                var bufferStart = existingRequest.StartDate.AddHours(-DomainConstants.RentalBufferHours);
                var bufferEnd = existingRequest.EndDate.AddHours(DomainConstants.RentalBufferHours);
                if (proposedStartDate < bufferEnd && proposedEndDate > bufferStart)
                {
                    return false;
                }
            }

            return true;
        }

        public void CreateConfirmedRental(int gameId, int renterUserId, int ownerUserId, DateTime rentalStartDate, DateTime rentalEndDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(rentalStartDate, rentalEndDate))
            {
                throw new ArgumentException("Start date must be before end date and not in the past.");
            }

            var gameToRent = gameLookupRepository.Get(gameId);
            if (gameToRent.Owner.Id != ownerUserId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }

            if (!IsSlotAvailable(gameId, rentalStartDate, rentalEndDate))
            {
                throw new InvalidOperationException(
                    $"Selected dates fall within the mandatory {DomainConstants.RentalBufferHours}-hour buffer of another rental.");
            }

            var confirmedRental = new Rental(
                id: NewRentalId,
                rentedGame: new Game { Id = gameId },
                renterUser: new User { Id = renterUserId },
                ownerUser: new User { Id = ownerUserId },
                startDate: rentalStartDate,
                endDate: rentalEndDate);

            rentalDataRepository.AddConfirmed(confirmedRental);

            var notificationTitle = Constants.NotificationTitles.RentalConfirmed;
            var notificationBody = $"Your rental for {gameToRent.Name} from {rentalStartDate:MMM dd} to {rentalEndDate:MMM dd} has been confirmed.";

            notificationEventService.SendNotificationToUser(renterUserId, new NotificationDTO
            {
                Title = notificationTitle,
                Body = notificationBody,
                Type = NotificationType.Informational
            });

            notificationEventService.SendNotificationToUser(ownerUserId, new NotificationDTO
            {
                Title = notificationTitle,
                Body = notificationBody,
                Type = NotificationType.Informational
            });
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(int renterUserId) =>
            rentalDataRepository
                .GetRentalsByRenter(renterUserId)
                .Select(rental => rentalDtoMapper.ToDTO(rental))
                .ToImmutableList();

        public ImmutableList<RentalDTO> GetRentalsForOwner(int ownerUserId) =>
            rentalDataRepository
                .GetRentalsByOwner(ownerUserId)
                .Select(rental => rentalDtoMapper.ToDTO(rental))
                .ToImmutableList();
    }
}
