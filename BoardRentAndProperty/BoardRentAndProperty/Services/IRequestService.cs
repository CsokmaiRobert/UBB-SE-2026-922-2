using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IRequestService
    {
        ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId);
        ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId);
        ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId);
        Result<int, CreateRequestError> CreateRequest(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate);
        Result<int, ApproveRequestError> ApproveRequest(int requestId, Guid ownerAccountId);
        Result<int, DenyRequestError> DenyRequest(int requestId, Guid ownerAccountId, string declineReason);
        Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId);
        void OnGameDeactivated(int gameId);
        bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate);
        ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(int gameId, int calendarMonth, int calendarYear);
        Result<int, OfferError> OfferGame(int requestId, Guid offeringOwnerAccountId);
    }
}
