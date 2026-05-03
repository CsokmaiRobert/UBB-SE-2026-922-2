using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using ApiRequestService = BoardRentAndProperty.Api.Services.IRequestService;
using ApiNotificationService = BoardRentAndProperty.Api.Services.INotificationService;
using ApiResultCreate = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.CreateRequestError>;
using ApiResultApprove = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.ApproveRequestError>;
using ApiResultDeny = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.DenyRequestError>;
using ApiResultCancel = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.CancelRequestError>;
using ApiResultOffer = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.OfferError>;
using BookedDateRange = BoardRentAndProperty.Api.Services.BookedDateRange;
using CreateRequestError = BoardRentAndProperty.Api.Services.CreateRequestError;
using ApproveRequestError = BoardRentAndProperty.Api.Services.ApproveRequestError;
using DenyRequestError = BoardRentAndProperty.Api.Services.DenyRequestError;
using CancelRequestError = BoardRentAndProperty.Api.Services.CancelRequestError;
using OfferError = BoardRentAndProperty.Api.Services.OfferError;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeApiRequestService : ApiRequestService
    {
        public int OnGameDeactivatedCallCount { get; private set; }
        public int LastDeactivatedGameId { get; private set; }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) =>
            ImmutableList<RequestDTO>.Empty;

        public ApiResultCreate CreateRequest(
            int gameId,
            Guid renterAccountId,
            Guid ownerAccountId,
            DateTime startDate,
            DateTime endDate) => ApiResultCreate.Failure(CreateRequestError.GameDoesNotExist);

        public ApiResultApprove ApproveRequest(int requestId, Guid ownerAccountId) =>
            ApiResultApprove.Failure(ApproveRequestError.NotFound);

        public ApiResultDeny DenyRequest(int requestId, Guid ownerAccountId, string declineReason) =>
            ApiResultDeny.Failure(DenyRequestError.NotFound);

        public ApiResultCancel CancelRequest(int requestId, Guid cancellingAccountId) =>
            ApiResultCancel.Failure(CancelRequestError.NotFound);

        public void OnGameDeactivated(int gameId)
        {
            this.OnGameDeactivatedCallCount++;
            this.LastDeactivatedGameId = gameId;
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate) => true;

        public ImmutableList<BookedDateRange> GetBookedDates(int gameId, int calendarMonth, int calendarYear) =>
            ImmutableList<BookedDateRange>.Empty;

        public ApiResultOffer OfferGame(int requestId, Guid offeringOwnerAccountId) =>
            ApiResultOffer.Failure(OfferError.NotFound);
    }

    internal sealed class FakeApiNotificationService : ApiNotificationService
    {
        public int DeleteLinkedNotificationCallCount { get; private set; }
        public int SendNotificationCallCount { get; private set; }
        public int LastLinkedRequestId { get; private set; }
        public Guid LastRecipientAccountId { get; private set; }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) =>
            ImmutableList<NotificationDTO>.Empty;

        public NotificationDTO GetNotificationByIdentifier(int notificationId) => new NotificationDTO { Id = notificationId };

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) => new NotificationDTO { Id = notificationId };

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto)
        {
        }

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto)
        {
            this.SendNotificationCallCount++;
            this.LastRecipientAccountId = recipientAccountId;
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            this.DeleteLinkedNotificationCallCount++;
            this.LastLinkedRequestId = relatedRequestId;
        }
    }
}
