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

        private readonly DispatcherQueue? uiDispatcherQueue;

        public Guid CurrentUserId { get; private set; }

        public NotificationsViewModel(
            INotificationService notificationLookupService,
            ICurrentUserContext currentUserContext)
        {
            this.notificationLookupService = notificationLookupService;

            try
            {
                uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            }
            catch (COMException)
            {
                uiDispatcherQueue = null;
            }

            LoadNotificationsForUser(currentUserContext.CurrentUserId);

            notificationSubscription = notificationLookupService.Subscribe(this);
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

        public void Dispose() => notificationSubscription?.Dispose();
    }
}
