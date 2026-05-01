using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Utilities;
namespace BoardRentAndProperty.Services
{
    public class RequestService : IRequestService
    {
        private const int NewRequestId = 0;
        private const int MissingForeignKeyId = 0;
        private const int MissingOptionalDatePart = 0;
        private const int AvailabilityWindowMonths = 1;
        private readonly IRequestRepository requestDataRepository;
        private readonly IRentalRepository rentalConflictRepository;
        private readonly INotificationService requestNotificationService;
        private readonly IGameRepository gameValidationRepository;
        private readonly IMapper<Request, RequestDTO, int> requestDtoMapper;
        public RequestService(IRequestRepository requestRepository, IRentalRepository rentalRepository, IGameRepository gameRepository,
            INotificationService notificationService, IMapper<Request, RequestDTO, int> requestMapper)
        {
            requestDataRepository = requestRepository;
            rentalConflictRepository = rentalRepository;
            gameValidationRepository = gameRepository;
            requestNotificationService = notificationService;
            requestDtoMapper = requestMapper;
        }
        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) =>
            requestDataRepository.GetRequestsByRenter(renterAccountId).Select(r => requestDtoMapper.ToDTO(r)).ToImmutableList();
        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) =>
            requestDataRepository.GetRequestsByOwner(ownerAccountId).Select(r => requestDtoMapper.ToDTO(r)).ToImmutableList();
        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) =>
            GetRequestsForOwner(ownerAccountId).Where(r => r.Status == RequestStatus.Open).ToImmutableList();
        public Result<int, CreateRequestError> CreateRequest(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (!DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate))
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.InvalidDateRange);
            }
            Game game;
            try
            {
                game = gameValidationRepository.Get(gameId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist);
            }

            var effectiveOwnerId = game.Owner?.Id ?? ownerAccountId;
            if (renterAccountId == effectiveOwnerId)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent);
            }
            if (!CheckAvailability(gameId, startDate, endDate))
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable);
            }

            var newRequest = new Request(NewRequestId, new Game { Id = gameId }, new Account { Id = renterAccountId }, new Account { Id = effectiveOwnerId }, startDate, endDate);
            requestDataRepository.Add(newRequest);
            return Result<int, CreateRequestError>.Success(newRequest.Id);
        }
        public Result<int, ApproveRequestError> ApproveRequest(int requestId, Guid ownerAccountId)
        {
            Request req;
            try
            {
                req = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }
            if (req.Owner?.Id != ownerAccountId)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.Unauthorized);
            }

            if (req.Status != RequestStatus.Open)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }
            if (!TryApproveOpenRequestAndNotify(req, out var rentalId))
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.TransactionFailed);
            }
            return Result<int, ApproveRequestError>.Success(rentalId);
        }
        public Result<int, DenyRequestError> DenyRequest(int requestId, Guid ownerAccountId, string denialReason)
        {
            Request req;
            try
            {
                req = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.NotFound);
            }
            if (req.Owner?.Id != ownerAccountId)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized);
            }

            var reason = string.IsNullOrWhiteSpace(denialReason?.Trim()) ? Constants.DialogMessages.NoReasonProvided : denialReason.Trim();
            requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            requestDataRepository.Delete(requestId);
            var renterId = req.Renter?.Id ?? Guid.Empty;
            var gameName = req.Game?.Name ?? "the selected game";
            SendNotificationToAccount(renterId, Constants.NotificationTitles.RentalRequestDeclined,
                $"Your request for {gameName} {FormatPeriod(req.StartDate, req.EndDate)} was declined. Reason: {reason}");
            return Result<int, DenyRequestError>.Success(requestId);
        }
        public Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId)
        {
            Request req;
            try
            {
                req = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);
            }
            if (req.Renter?.Id != cancellingAccountId)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.Unauthorized);
            }
            requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            try
            {
                requestDataRepository.Delete(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CancelRequestError>.Failure(CancelRequestError.NotFound);
            }
            return Result<int, CancelRequestError>.Success(requestId);
        }
        public void OnGameDeactivated(int deactivatedGameId)
        {
            var pending = requestDataRepository.GetRequestsByGame(deactivatedGameId)
                .Where(r => r.Status == RequestStatus.Open || r.Status == RequestStatus.OfferPending).ToImmutableList();
            foreach (var r in pending)
            {
                requestNotificationService.DeleteNotificationsLinkedToRequest(r.Id);
                requestDataRepository.Delete(r.Id);
                var renterId = r.Renter?.Id ?? Guid.Empty;
                var gameName = r.Game?.Name ?? "the selected game";
                SendNotificationToAccount(renterId, Constants.NotificationTitles.RentalRequestCancelled,
                    $"Your request for {gameName} {FormatPeriod(r.StartDate, r.EndDate)} has been cancelled because the game is no longer available.");
            }
        }
        public ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(int gameId, int month = MissingOptionalDatePart, int year = MissingOptionalDatePart)
        {
            if (month == MissingOptionalDatePart)
            {
                month = DateTime.UtcNow.Month;
            }
            if (year == MissingOptionalDatePart)
            {
                year = DateTime.UtcNow.Year;
            }
            return requestDataRepository.GetRequestsByGame(gameId)
                .Where(r => r.StartDate.Month == month && r.StartDate.Year == year)
                .OrderBy(r => r.StartDate)
                .Select(r => (r.StartDate, r.EndDate.AddHours(DomainConstants.RentalBufferHours)))
                .ToImmutableList();
        }
        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            if (startDate > DateTime.UtcNow.AddMonths(AvailabilityWindowMonths) || endDate > DateTime.UtcNow.AddMonths(AvailabilityWindowMonths))
            {
                return false;
            }
            Game game;
            try
            {
                game = gameValidationRepository.Get(gameId);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            if (!game.IsActive)
            {
                return false;
            }

            bool rentalConflict = rentalConflictRepository.GetRentalsByGame(gameId)
                .Any(r => startDate < r.EndDate.AddHours(DomainConstants.RentalBufferHours) && endDate > r.StartDate.AddHours(-DomainConstants.RentalBufferHours));
            if (rentalConflict)
            {
                return false;
            }
            return !requestDataRepository.GetRequestsByGame(gameId)
                .Any(r => r.StartDate.AddHours(-DomainConstants.RentalBufferHours) < endDate && r.EndDate.AddHours(DomainConstants.RentalBufferHours) > startDate);
        }
        public Result<int, OfferError> OfferGame(int requestId, Guid offeringOwnerAccountId)
        {
            Request req;
            try
            {
                req = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, OfferError>.Failure(OfferError.NotFound);
            }
            if (req.Owner?.Id != offeringOwnerAccountId)
            {
                return Result<int, OfferError>.Failure(OfferError.NotOwner);
            }
            if (req.Status != RequestStatus.Open)
            {
                return Result<int, OfferError>.Failure(OfferError.RequestNotOpen);
            }
            if (!TryApproveOpenRequestAndNotify(req, out var rentalId))
            {
                return Result<int, OfferError>.Failure(OfferError.TransactionFailed);
            }
            return Result<int, OfferError>.Success(rentalId);
        }
        private void SendNotificationToAccount(Guid accountId, string title, string body, NotificationType type = default, int? relatedRequestId = null)
        {
            if (accountId == Guid.Empty)
            {
                return;
            }
            requestNotificationService.SendNotificationToUser(accountId, new NotificationDTO
            {
                Id = NewRequestId, Recipient = new UserDTO { Id = accountId }, Timestamp = DateTime.UtcNow,
                Title = title, Body = body, Type = type, RelatedRequestId = relatedRequestId
            });
        }
        private bool TryApproveOpenRequestAndNotify(Request req, out int rentalId)
        {
            var buffStart = req.StartDate.AddHours(-DomainConstants.RentalBufferHours);
            var buffEnd = req.EndDate.AddHours(DomainConstants.RentalBufferHours);
            var conflicts = requestDataRepository.GetOverlappingRequests(req.Game?.Id ?? MissingForeignKeyId, req.Id, buffStart, buffEnd);
            try
            {
                rentalId = requestDataRepository.ApproveAtomically(req, conflicts);
            }
            catch
            {
                rentalId = MissingForeignKeyId;
                return false;
            }
            var gameName = req.Game?.Name ?? "the selected game";
            foreach (var c in conflicts)
            {
                var cRenterId = c.Renter?.Id ?? Guid.Empty;
                SendNotificationToAccount(cRenterId, Constants.NotificationTitles.BookingUnavailable,
                    $"Your request for {gameName} {FormatPeriod(c.StartDate, c.EndDate)} was declined because the game is no longer available in that period.");
            }
            var renterId = req.Renter?.Id ?? Guid.Empty;
            SendNotificationToAccount(renterId, Constants.NotificationTitles.RentalRequestApproved,
                $"Your request for {gameName} {FormatPeriod(req.StartDate, req.EndDate)} was approved.");
            requestNotificationService.ScheduleUpcomingRentalReminder(renterId, req.Owner?.Id ?? Guid.Empty, req.Game?.Name ?? "your game", req.StartDate);
            return true;
        }
        private static string FormatPeriod(DateTime start, DateTime end) => $"({start:d}-{end:d})";
    }
}
