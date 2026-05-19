using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using ApiNotificationService = BoardRentAndProperty.ApiClient.INotificationService;
using CurrentUserContextInterface = BoardRentAndProperty.Utilities.ICurrentUserContext;

namespace BoardRentAndProperty.Services
{
    public sealed class DesktopNotificationService :
        IDesktopNotificationService,
        IObserver<IncomingNotification>,
        IDisposable
    {
        private const int NewNotificationId = 0;

        private readonly ApiNotificationService apiNotificationService;
        private readonly IServerClient serverNotificationClient;
        private readonly CurrentUserContextInterface currentUserContext;
        private readonly IToastNotificationService toastNotificationService;
        private readonly List<IObserver<NotificationDTO>> notificationObservers = new();
        private readonly object notificationObserversLock = new();
        private bool isDisposed;

        public DesktopNotificationService(
            ApiNotificationService apiNotificationService,
            IServerClient serverNotificationClient,
            CurrentUserContextInterface currentUserContext,
            IToastNotificationService toastNotificationService)
        {
            this.apiNotificationService = apiNotificationService;
            this.serverNotificationClient = serverNotificationClient;
            this.currentUserContext = currentUserContext;
            this.toastNotificationService = toastNotificationService;
            this.serverNotificationClient.Subscribe(this);
        }

        public Task<ServiceResult<NotificationDTO>> GetNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default) =>
            this.apiNotificationService.GetNotificationByIdentifierAsync(notificationId, cancellationToken);

        public Task<ServiceResult<NotificationDTO>> DeleteNotificationByIdentifierAsync(
            int notificationId,
            CancellationToken cancellationToken = default) =>
            this.apiNotificationService.DeleteNotificationByIdentifierAsync(notificationId, cancellationToken);

        public Task<ServiceResult> UpdateNotificationByIdentifierAsync(
            int notificationId,
            NotificationDTO updatedNotification,
            CancellationToken cancellationToken = default) =>
            this.apiNotificationService.UpdateNotificationByIdentifierAsync(
                notificationId,
                updatedNotification,
                cancellationToken);

        public async Task<ServiceResult> SendNotificationToUserAsync(
            Guid recipientAccountId,
            NotificationDTO notification,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(notification);

            DateTime timestamp = notification.Timestamp == default ? DateTime.UtcNow : notification.Timestamp;
            var notificationToPersist = new NotificationDTO
            {
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = recipientAccountId },
                Timestamp = timestamp,
                Title = notification.Title,
                Body = notification.Body,
                Type = notification.Type,
                RelatedRequestId = notification.RelatedRequestId,
            };

            var persistResult = await this.apiNotificationService.PersistNotificationAsync(
                notificationToPersist,
                cancellationToken);
            if (!persistResult.Success)
            {
                return persistResult;
            }

            if (this.currentUserContext.CurrentUserId == recipientAccountId)
            {
                this.NotifyObservers(notificationToPersist);
                this.toastNotificationService.Show(notification.Title, notification.Body);
                return ServiceResult.Ok();
            }

            this.serverNotificationClient.SendNotification(
                ToServerInt(recipientAccountId),
                notification.Title,
                notification.Body);

            return ServiceResult.Ok();
        }

        public Task<ServiceResult<IReadOnlyList<NotificationDTO>>> GetNotificationsForUserAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            this.apiNotificationService.GetNotificationsForUserAsync(accountId, cancellationToken);

        public Task<ServiceResult> DeleteNotificationsLinkedToRequestAsync(
            int relatedRequestId,
            CancellationToken cancellationToken = default) =>
            this.apiNotificationService.DeleteNotificationsLinkedToRequestAsync(relatedRequestId, cancellationToken);

        public void SubscribeToServer(Guid accountId) =>
            this.serverNotificationClient.SubscribeToServer(ToServerInt(accountId));

        public void StartListening() =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await this.serverNotificationClient.ListenAsync();
                }
                catch (System.Net.Sockets.SocketException socketException)
                {
                    System.Diagnostics.Debug.WriteLine($"DesktopNotificationService: listen terminated - {socketException}");
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    System.Diagnostics.Debug.WriteLine($"DesktopNotificationService: listen terminated - {invalidOperationException}");
                }
            });

        public void StopListening() => this.serverNotificationClient.StopListening();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(IncomingNotification incomingNotification)
        {
            var notification = new NotificationDTO
            {
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = Guid.Empty },
                Timestamp = incomingNotification.Timestamp,
                Title = incomingNotification.Title,
                Body = incomingNotification.Body,
            };

            this.NotifyObservers(notification);
            this.toastNotificationService.Show(incomingNotification.Title, incomingNotification.Body);
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer)
        {
            lock (this.notificationObserversLock)
            {
                this.notificationObservers.Add(observer);
            }

            return new NotificationObserverSubscription(
                this.notificationObservers,
                this.notificationObserversLock,
                observer);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.StopListening();
            (this.serverNotificationClient as IDisposable)?.Dispose();
        }

        private static int ToServerInt(Guid accountId) => Math.Abs(accountId.GetHashCode());

        private void NotifyObservers(NotificationDTO notification)
        {
            IObserver<NotificationDTO>[] observerSnapshot;
            lock (this.notificationObserversLock)
            {
                observerSnapshot = this.notificationObservers.ToArray();
            }

            foreach (var notificationObserver in observerSnapshot)
            {
                notificationObserver.OnNext(notification);
            }
        }

        private sealed class NotificationObserverSubscription : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> observers;
            private readonly object observersLock;
            private readonly IObserver<NotificationDTO> observerToRemove;

            public NotificationObserverSubscription(
                List<IObserver<NotificationDTO>> observers,
                object observersLock,
                IObserver<NotificationDTO> observerToRemove)
            {
                this.observers = observers;
                this.observersLock = observersLock;
                this.observerToRemove = observerToRemove;
            }

            public void Dispose()
            {
                lock (this.observersLock)
                {
                    this.observers.Remove(this.observerToRemove);
                }
            }
        }
    }
}
