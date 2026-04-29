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
        private const int InvalidOrUnknownUserId = 0;

        private readonly INotificationService notificationLookupService;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IDisposable notificationSubscription;

        private readonly DispatcherQueue? uiDispatcherQueue;
        private bool hasConnectionWarning;
        private string connectionWarningMessage = string.Empty;

        public int CurrentUserId { get; private set; }

        public bool HasConnectionWarning
        {
            get => hasConnectionWarning;
            private set
            {
                if (hasConnectionWarning != value)
                {
                    hasConnectionWarning = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionWarningMessage
        {
            get => connectionWarningMessage;
            private set
            {
                if (connectionWarningMessage != value)
                {
                    connectionWarningMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public NotificationsViewModel(
            INotificationService notificationLookupService,
            ICurrentUserContext currentUserContext)
        {
            this.notificationLookupService = notificationLookupService;
            this.currentUserContext = currentUserContext;

            try
            {
                uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            }
            catch (COMException)
            {
                uiDispatcherQueue = null;
            }

            LoadCurrentUserNotifications();

            notificationSubscription = notificationLookupService.Subscribe(this);
            notificationLookupService.ConnectionStatusChanged += OnConnectionStatusChanged;
            ApplyConnectionStatus(notificationLookupService.ConnectionStatus);
        }

        public void LoadCurrentUserNotifications()
        {
            LoadNotificationsForUser(currentUserContext.CurrentUserId);
        }

        public void LoadNotificationsForUser(int targetUserId)
        {
            CurrentUserId = targetUserId;
            Reload();
        }

        protected override void Reload()
        {
            if (CurrentUserId == InvalidOrUnknownUserId)
            {
                SetAllItems(ImmutableList<NotificationDTO>.Empty);
                return;
            }

            var userNotificationsSortedByNewest = notificationLookupService
                .GetNotificationsForUser(CurrentUserId)
                .OrderByDescending(notification => notification.Timestamp)
                .ThenByDescending(notification => notification.Id)
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
            var resolvedCurrentUserId = currentUserContext.CurrentUserId;
            if (resolvedCurrentUserId == InvalidOrUnknownUserId)
            {
                LoadNotificationsForUser(InvalidOrUnknownUserId);
                return;
            }

            if (incomingNotification?.User?.Id != InvalidOrUnknownUserId
                && incomingNotification?.User?.Id != resolvedCurrentUserId)
            {
                return;
            }

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => LoadNotificationsForUser(resolvedCurrentUserId));
                return;
            }

            LoadNotificationsForUser(resolvedCurrentUserId);
        }

        private void OnConnectionStatusChanged(object? sender, NotificationConnectionStatusChangedEventArgs eventArgs)
        {
            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => ApplyConnectionStatus(eventArgs.ConnectionStatus));
                return;
            }

            ApplyConnectionStatus(eventArgs.ConnectionStatus);
        }

        private void ApplyConnectionStatus(NotificationConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case NotificationConnectionStatus.Reconnecting:
                    ConnectionWarningMessage = "Notifications are reconnecting.";
                    HasConnectionWarning = true;
                    break;
                case NotificationConnectionStatus.Offline:
                    ConnectionWarningMessage = "Notifications are offline. New alerts will appear after reconnecting.";
                    HasConnectionWarning = true;
                    break;
                default:
                    ConnectionWarningMessage = string.Empty;
                    HasConnectionWarning = false;
                    break;
            }
        }

        public void Dispose()
        {
            notificationLookupService.ConnectionStatusChanged -= OnConnectionStatusChanged;
            notificationSubscription?.Dispose();
        }
    }
}
