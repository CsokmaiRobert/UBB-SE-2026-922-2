using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class NotificationsViewModel : PagedViewModel<NotificationDTO>,
                                           IObserver<NotificationDTO>,
                                           IDisposable
    {
        private static readonly Guid InvalidOrUnknownUserId = Guid.Empty;

        private readonly INotificationService notificationLookupService;
        private readonly IDisposable notificationSubscription;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IServerClient serverClient;

        private readonly DispatcherQueue? uiDispatcherQueue;
        private NotificationConnectionStatus currentConnectionStatus;

        public Guid CurrentUserId { get; private set; }
        public bool HasConnectionWarning =>
            currentConnectionStatus == NotificationConnectionStatus.Offline
            || currentConnectionStatus == NotificationConnectionStatus.Reconnecting;

        public string ConnectionWarningMessage => currentConnectionStatus switch
        {
            NotificationConnectionStatus.Offline => "Notification server is offline. You can keep using the app, but live notifications are temporarily unavailable.",
            NotificationConnectionStatus.Reconnecting => "Reconnecting to the notification server...",
            _ => string.Empty,
        };

        public NotificationsViewModel(
            INotificationService notificationLookupService,
            ICurrentUserContext currentUserContext,
            IServerClient serverClient)
        {
            this.notificationLookupService = notificationLookupService;
            this.currentUserContext = currentUserContext;
            this.serverClient = serverClient;
            this.currentConnectionStatus = serverClient.ConnectionStatus;

            try
            {
                uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            }
            catch (COMException)
            {
                uiDispatcherQueue = null;
            }

            this.serverClient.ConnectionStatusChanged += this.OnConnectionStatusChanged;
            LoadNotificationsForUser(currentUserContext.CurrentUserId);

            notificationSubscription = notificationLookupService.Subscribe(this);
        }

        public void LoadCurrentUserNotifications()
        {
            LoadNotificationsForUser(this.currentUserContext.CurrentUserId);
        }

        public void LoadNotificationsForUser(Guid targetUserId)
        {
            CurrentUserId = targetUserId;
            Reload();
        }

        protected override void Reload()
        {
            var userNotificationsSortedByNewest = notificationLookupService
                .GetNotificationsForUser(CurrentUserId)
                .OrderByDescending(notification => notification.Id)
                .ToImmutableList();

            SetAllItems(userNotificationsSortedByNewest);
        }

        public void DeleteNotificationByIdentifier(int notificationIdToDelete)
        {
            try
            {
                notificationLookupService.DeleteNotificationByIdentifier(notificationIdToDelete);
            }
            catch (KeyNotFoundException)
            {
            }

            Reload();
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception observableError)
        {
            System.Diagnostics.Debug.WriteLine($"Notification observable error: {observableError.Message}");
        }

        public void OnNext(NotificationDTO incomingNotification)
        {
            if (CurrentUserId == InvalidOrUnknownUserId) return;

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => LoadNotificationsForUser(CurrentUserId));
                return;
            }

            LoadNotificationsForUser(CurrentUserId);
        }

        public void Dispose()
        {
            notificationSubscription?.Dispose();
            this.serverClient.ConnectionStatusChanged -= this.OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(object? sender, NotificationConnectionStatusChangedEventArgs eventArgs)
        {
            void ApplyStatus()
            {
                currentConnectionStatus = eventArgs.ConnectionStatus;
                OnPropertyChanged(nameof(HasConnectionWarning));
                OnPropertyChanged(nameof(ConnectionWarningMessage));
            }

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(ApplyStatus);
                return;
            }

            ApplyStatus();
        }
    }
}
