using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class NotificationsViewModel : PagedViewModel<NotificationDTO>,
                                           IObserver<NotificationDTO>,
                                           IDisposable
    {
        private static readonly Guid InvalidOrUnknownUserId = Guid.Empty;

        private readonly IDesktopNotificationService notificationLookupService;
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
            IDesktopNotificationService notificationLookupService,
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

            if (currentUserContext.CurrentUserId != InvalidOrUnknownUserId)
            {
                _ = this.LoadNotificationsForUserAsync(currentUserContext.CurrentUserId);
            }

            notificationSubscription = notificationLookupService.Subscribe(this);
        }

        public Task LoadCurrentUserNotificationsAsync()
        {
            return this.LoadNotificationsForUserAsync(this.currentUserContext.CurrentUserId);
        }

        public async Task LoadNotificationsForUserAsync(Guid targetUserId)
        {
            CurrentUserId = targetUserId;
            await this.ReloadAsync();
        }

        protected override void Reload()
        {
            _ = this.ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            var notificationsResult = await this.notificationLookupService.GetNotificationsForUserAsync(CurrentUserId);
            var userNotificationsSortedByNewest = notificationsResult.Success && notificationsResult.Data != null
                ? notificationsResult.Data.OrderByDescending(notification => notification.Id).ToImmutableList()
                : ImmutableList<NotificationDTO>.Empty;

            SetAllItems(userNotificationsSortedByNewest);
        }

        public async Task DeleteNotificationByIdentifierAsync(int notificationIdToDelete)
        {
            await this.notificationLookupService.DeleteNotificationByIdentifierAsync(notificationIdToDelete);
            await this.ReloadAsync();
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
            if (CurrentUserId == InvalidOrUnknownUserId)
            {
                return;
            }

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => _ = LoadNotificationsForUserAsync(CurrentUserId));
                return;
            }

            _ = LoadNotificationsForUserAsync(CurrentUserId);
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
