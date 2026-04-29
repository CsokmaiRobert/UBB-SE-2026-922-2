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

        public int CurrentUserId { get; private set; }

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

        public void Dispose() => notificationSubscription?.Dispose();
    }
}
