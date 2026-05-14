using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeClientRequestService : IRequestService
    {
        public ImmutableList<RequestDTO> RequestsForRenter { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> RequestsForOwner { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> OpenRequestsForOwner { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<(DateTime StartDate, DateTime EndDate)> BookedDates { get; set; } =
            ImmutableList<(DateTime StartDate, DateTime EndDate)>.Empty;
        public Result<int, CreateRequestError> CreateRequestResult { get; set; } =
            Result<int, CreateRequestError>.Success(1);
        public Result<int, ApproveRequestError> ApproveRequestResult { get; set; } =
            Result<int, ApproveRequestError>.Success(1);
        public Result<int, DenyRequestError> DenyRequestResult { get; set; } =
            Result<int, DenyRequestError>.Success(1);
        public Result<int, CancelRequestError> CancelRequestResult { get; set; } =
            Result<int, CancelRequestError>.Success(1);
        public Result<int, OfferError> OfferGameResult { get; set; } =
            Result<int, OfferError>.Success(1);
        public bool AvailabilityResult { get; set; } = true;
        public int CreateRequestCallCount { get; private set; }
        public int CancelRequestCallCount { get; private set; }
        public int ApproveRequestCallCount { get; private set; }
        public int DenyRequestCallCount { get; private set; }
        public int LastRequestId { get; private set; }
        public int LastGameId { get; private set; }
        public Guid LastRenterAccountId { get; private set; }
        public Guid LastOwnerAccountId { get; private set; }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) => this.RequestsForRenter;

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) => this.RequestsForOwner;

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) => this.OpenRequestsForOwner;

        public Result<int, CreateRequestError> CreateRequest(
            int gameId,
            Guid renterAccountId,
            Guid ownerAccountId,
            DateTime startDate,
            DateTime endDate)
        {
            this.CreateRequestCallCount++;
            this.LastGameId = gameId;
            this.LastRenterAccountId = renterAccountId;
            this.LastOwnerAccountId = ownerAccountId;
            return this.CreateRequestResult;
        }

        public Result<int, ApproveRequestError> ApproveRequest(int requestId, Guid ownerAccountId)
        {
            this.ApproveRequestCallCount++;
            this.LastRequestId = requestId;
            this.LastOwnerAccountId = ownerAccountId;
            return this.ApproveRequestResult;
        }

        public Result<int, DenyRequestError> DenyRequest(int requestId, Guid ownerAccountId, string declineReason)
        {
            this.DenyRequestCallCount++;
            this.LastRequestId = requestId;
            this.LastOwnerAccountId = ownerAccountId;
            return this.DenyRequestResult;
        }

        public Result<int, CancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId)
        {
            this.CancelRequestCallCount++;
            this.LastRequestId = requestId;
            return this.CancelRequestResult;
        }

        public void OnGameDeactivated(int gameId)
        {
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate) => this.AvailabilityResult;

        public ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(
            int gameId,
            int calendarMonth,
            int calendarYear) => this.BookedDates;

        public Result<int, OfferError> OfferGame(int requestId, Guid offeringOwnerAccountId) =>
            this.OfferGameResult;
    }

    internal sealed class FakeClientNotificationService : INotificationService
    {
        public ImmutableList<NotificationDTO> NotificationsForUser { get; set; } =
            ImmutableList<NotificationDTO>.Empty;
        public int SendNotificationCallCount { get; private set; }
        public int DeleteNotificationCallCount { get; private set; }
        public int DeleteLinkedNotificationCallCount { get; private set; }
        public Guid LastRecipientAccountId { get; private set; }
        public int LastDeletedNotificationId { get; private set; }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer) => new EmptyDisposable();

        public NotificationDTO GetNotificationByIdentifier(int notificationId) => new NotificationDTO { Id = notificationId };

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId)
        {
            this.DeleteNotificationCallCount++;
            this.LastDeletedNotificationId = notificationId;
            return new NotificationDTO { Id = notificationId };
        }

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto)
        {
        }

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto)
        {
            this.SendNotificationCallCount++;
            this.LastRecipientAccountId = recipientAccountId;
        }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) => this.NotificationsForUser;

        public void SubscribeToServer(Guid accountId)
        {
        }

        public void StartListening()
        {
        }

        public void StopListening()
        {
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
            this.DeleteLinkedNotificationCallCount++;
        }
    }

    internal sealed class FakeServerClient : IServerClient
    {
        public event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public NotificationConnectionStatus ConnectionStatus { get; set; }
        public int SubscribeToServerCallCount { get; private set; }
        public int StopListeningCallCount { get; private set; }
        public int LastTargetUserId { get; private set; }

        public IDisposable Subscribe(IObserver<IncomingNotification> observer) => new EmptyDisposable();

        public Task ListenAsync() => Task.CompletedTask;

        public void SubscribeToServer(int targetUserId)
        {
            this.SubscribeToServerCallCount++;
            this.LastTargetUserId = targetUserId;
        }

        public void SendNotification(int targetUserId, string notificationTitle, string notificationBody)
        {
        }

        public void StopListening()
        {
            this.StopListeningCallCount++;
        }

        public void RaiseConnectionStatusChanged(NotificationConnectionStatus status)
        {
            this.ConnectionStatus = status;
            this.ConnectionStatusChanged?.Invoke(
                this,
                new NotificationConnectionStatusChangedEventArgs(status));
        }
    }

    internal sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
