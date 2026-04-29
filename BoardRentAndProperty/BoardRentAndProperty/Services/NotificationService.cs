using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public class NotificationService : INotificationService, IObserver<IncomingNotification>, IObservable<NotificationDTO>, IDisposable
    {
        private static readonly TimeSpan UpcomingRentalReminderLeadTime = TimeSpan.FromHours(24);
        private const int NewNotificationId = 0;
        private const int MissingUserId = 0;

        private bool isDisposed;
        private readonly INotificationRepository notificationDataRepository;
        private readonly IMapper<Notification, NotificationDTO> notificationDtoMapper;
        private readonly IServerClient serverNotificationClient;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IToastNotificationService toastAlertService;
        private Task? serverListeningTask;

        private readonly List<IObserver<NotificationDTO>> notificationSubscribers = new();
        private readonly object notificationSubscribersLock = new();
        private readonly Dictionary<int, ScheduledReminder> scheduledRemindersByRentalId = new();
        private readonly object scheduledRemindersLock = new();

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper<Notification, NotificationDTO> notificationMapper,
            IServerClient serverClient,
            ICurrentUserContext currentUserContext,
            IToastNotificationService toastNotificationService)
        {
            this.notificationDataRepository = notificationRepository;
            this.notificationDtoMapper = notificationMapper;
            this.serverNotificationClient = serverClient;
            this.currentUserContext = currentUserContext;
            this.toastAlertService = toastNotificationService;
            serverNotificationClient.Subscribe(this);
        }

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Delete(notificationId));

        public NotificationDTO GetNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Get(notificationId));

        public ImmutableList<NotificationDTO> GetNotificationsForUser(int targetUserId) =>
            notificationDataRepository
                .GetNotificationsByUser(targetUserId)
                .Select(notification => notificationDtoMapper.ToDTO(notification))
                .OrderByDescending(notification => notification.Timestamp)
                .ThenByDescending(notification => notification.Id)
                .ToImmutableList();

        public void SendNotificationToUser(int recipientUserId, NotificationDTO notificationToSend)
        {
            if (notificationToSend == null)
            {
                throw new ArgumentNullException(nameof(notificationToSend));
            }

            DateTime notificationTimestamp = notificationToSend.Timestamp == default ? DateTime.UtcNow : notificationToSend.Timestamp;

            var persistedNotification = PersistNotification(
                recipientUserId,
                notificationTimestamp,
                notificationToSend.Title,
                notificationToSend.Body,
                notificationToSend.Type,
                notificationToSend.RelatedRequestId);

            BroadcastNotificationForCurrentUser(recipientUserId, persistedNotification);
            PushNotificationToServer(recipientUserId, notificationToSend.Title, notificationToSend.Body);
        }

        private Notification PersistNotification(
            int recipientUserId,
            DateTime notificationTimestamp,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType,
            int? linkedRequestId)
        {
            var notificationModel = BuildNotificationDomainModel(
                recipientUserId,
                notificationTimestamp,
                notificationTitle,
                notificationBody,
                notificationType,
                linkedRequestId);

            notificationDataRepository.Add(notificationModel);
            return notificationModel;
        }

        private void BroadcastNotificationForCurrentUser(int recipientUserId, Notification notificationModel)
        {
            if (currentUserContext.CurrentUserId == recipientUserId)
            {
                NotifyAllSubscribers(BuildNotificationDataTransferObject(
                    notificationModel.Id,
                    recipientUserId,
                    notificationModel.Timestamp,
                    notificationModel.Title,
                    notificationModel.Body,
                    notificationModel.Type,
                    notificationModel.RelatedRequestId));
            }
        }

        private void PushNotificationToServer(int recipientUserId, string notificationTitle, string notificationBody)
        {
            try
            {
                serverNotificationClient.SendNotification(recipientUserId, notificationTitle, notificationBody);
            }
            catch (Exception serverPushException)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"NotificationService: server push failed - {serverPushException}");
            }
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId)
        {
            notificationDataRepository.DeleteNotificationsLinkedToRequest(linkedRequestId);
        }

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationData)
        {
            notificationDataRepository.Update(notificationId, notificationDtoMapper.ToModel(updatedNotificationData));
        }

        public void StartListening()
        {
            if (serverListeningTask is { IsCompleted: false })
            {
                return;
            }

            serverListeningTask = Task.Run(async () =>
            {
                try
                {
                    await serverNotificationClient.ListenAsync();
                }
                catch (Exception listenException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"NotificationService: listen loop terminated - {listenException}");
                }
            });
        }

        public void StopListening() => serverNotificationClient.StopListening();
        public void OnCompleted()
        {
        }
        public void OnError(Exception observableError)
        {
        }

        public void OnNext(IncomingNotification receivedNotification)
        {
            var incomingNotificationDto = BuildNotificationDataTransferObject(
                NewNotificationId,
                receivedNotification.UserId,
                receivedNotification.Timestamp,
                receivedNotification.Title,
                receivedNotification.Body,
                default,
                null);

            NotifyAllSubscribers(incomingNotificationDto);
            toastAlertService.Show(receivedNotification.Title, receivedNotification.Body);
        }

        private void NotifyAllSubscribers(NotificationDTO outgoingNotificationDto)
        {
            IObserver<NotificationDTO>[] subscribersSnapshot;
            lock (notificationSubscribersLock)
            {
                subscribersSnapshot = notificationSubscribers.ToArray();
            }

            foreach (var subscriber in subscribersSnapshot)
            {
                subscriber.OnNext(outgoingNotificationDto);
            }
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> newObserver)
        {
            lock (notificationSubscribersLock)
            {
                notificationSubscribers.Add(newObserver);
            }

            return new Unsubscriber(notificationSubscribers, notificationSubscribersLock, newObserver);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> subscribersList;
            private readonly object subscribersListLock;
            private readonly IObserver<NotificationDTO> subscriberToRemove;

            public Unsubscriber(
                List<IObserver<NotificationDTO>> subscribers,
                object subscribersLock,
                IObserver<NotificationDTO> observer)
            {
                this.subscribersList = subscribers;
                this.subscribersListLock = subscribersLock;
                this.subscriberToRemove = observer;
            }

            public void Dispose()
            {
                lock (subscribersListLock)
                {
                    subscribersList.Remove(subscriberToRemove);
                }
            }
        }

        public void SubscribeToServer(int userId) => serverNotificationClient.SubscribeToServer(userId);

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            CancelAllScheduledReminders();
            StopListening();
            (serverNotificationClient as IDisposable)?.Dispose();
        }

        public void ScheduleUpcomingRentalReminder(int rentalId, int renterUserId, int ownerUserId, string rentalGameName, DateTime rentalStartDate)
        {
            if (isDisposed || rentalId == NewNotificationId)
            {
                return;
            }

            var rentalStartUtc = rentalStartDate.ToUniversalTime();
            var currentUtcTime = DateTime.UtcNow;

            if (rentalStartUtc <= currentUtcTime)
            {
                return;
            }

            DateTime scheduledReminderTime = rentalStartUtc - UpcomingRentalReminderLeadTime;
            string reminderTitle = Constants.NotificationTitles.UpcomingRentalReminder;
            string reminderBody = BuildUpcomingRentalReminderBody(rentalGameName, rentalStartDate);

            ScheduleOrSendReminderForRental(
                rentalId,
                renterUserId,
                ownerUserId,
                reminderTitle,
                reminderBody,
                scheduledReminderTime);
        }

        public void CancelUpcomingRentalReminder(int rentalId)
        {
            ScheduledReminder? scheduledReminderToCancel = null;
            lock (scheduledRemindersLock)
            {
                if (scheduledRemindersByRentalId.TryGetValue(rentalId, out scheduledReminderToCancel))
                {
                    scheduledRemindersByRentalId.Remove(rentalId);
                }
            }

            scheduledReminderToCancel?.Cancel();
        }

        private static string BuildUpcomingRentalReminderBody(string rentalGameName, DateTime rentalStartDate)
        {
            return $"Game: {rentalGameName}\nStart: {rentalStartDate:dd/MM/yyyy HH:mm}\n" +
                   "Delivery/Pick-up: Coordinate delivery/pick-up directly with the other party.";
        }

        private void ScheduleOrSendReminderForRental(
            int rentalId,
            int renterUserId,
            int ownerUserId,
            string reminderTitle,
            string reminderBody,
            DateTime scheduledSendTime)
        {
            TimeSpan sendDelay = scheduledSendTime.ToUniversalTime() - DateTime.UtcNow;
            if (sendDelay <= TimeSpan.Zero)
            {
                SendReminderNotificationsImmediately(renterUserId, ownerUserId, reminderTitle, reminderBody);
                return;
            }

            CancelUpcomingRentalReminder(rentalId);

            var cancellationSource = new CancellationTokenSource();
            var scheduledReminder = new ScheduledReminder(cancellationSource);

            lock (scheduledRemindersLock)
            {
                if (isDisposed)
                {
                    scheduledReminder.Cancel();
                    scheduledReminder.Dispose();
                    return;
                }

                scheduledRemindersByRentalId[rentalId] = scheduledReminder;
            }

            scheduledReminder.RunningTask = SendScheduledReminderAsync(
                rentalId,
                renterUserId,
                ownerUserId,
                reminderTitle,
                reminderBody,
                sendDelay,
                cancellationSource.Token)
                .ContinueWith(
                    completedTask => CompleteScheduledReminder(rentalId, scheduledReminder),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }

        private async Task SendScheduledReminderAsync(
            int rentalId,
            int renterUserId,
            int ownerUserId,
            string reminderTitle,
            string reminderBody,
            TimeSpan sendDelay,
            CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(sendDelay, cancellationToken);
                SendReminderNotificationsImmediately(renterUserId, ownerUserId, reminderTitle, reminderBody);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception scheduledReminderException)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"NotificationService: scheduled reminder failed for rental {rentalId} - {scheduledReminderException}");
            }
        }

        private void SendReminderNotificationsImmediately(int renterUserId, int ownerUserId, string reminderTitle, string reminderBody)
        {
            SendReminderNotificationIfRecipientExists(renterUserId, reminderTitle, reminderBody);
            SendReminderNotificationIfRecipientExists(ownerUserId, reminderTitle, reminderBody);
        }

        private void SendReminderNotificationIfRecipientExists(int recipientUserId, string reminderTitle, string reminderBody)
        {
            if (recipientUserId == MissingUserId)
            {
                return;
            }

            var immediateReminderDto = BuildNotificationDataTransferObject(
                NewNotificationId,
                recipientUserId,
                DateTime.UtcNow,
                reminderTitle,
                reminderBody,
                default,
                null);

            SendNotificationToUser(recipientUserId, immediateReminderDto);
        }

        private void CompleteScheduledReminder(int rentalId, ScheduledReminder completedReminder)
        {
            lock (scheduledRemindersLock)
            {
                if (scheduledRemindersByRentalId.TryGetValue(rentalId, out var trackedReminder)
                    && ReferenceEquals(trackedReminder, completedReminder))
                {
                    scheduledRemindersByRentalId.Remove(rentalId);
                }
            }

            completedReminder.Dispose();
        }

        private void CancelAllScheduledReminders()
        {
            ScheduledReminder[] remindersToCancel;
            lock (scheduledRemindersLock)
            {
                remindersToCancel = scheduledRemindersByRentalId.Values.ToArray();
                scheduledRemindersByRentalId.Clear();
            }

            foreach (var scheduledReminder in remindersToCancel)
            {
                scheduledReminder.Cancel();
            }
        }

        private static NotificationDTO BuildNotificationDataTransferObject(
            int notificationId,
            int recipientUserId,
            DateTime notificationTimestamp,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType,
            int? linkedRequestId)
        {
            return new NotificationDTO
            {
                Id = notificationId,
                User = new UserDTO { Id = recipientUserId },
                Timestamp = notificationTimestamp,
                Title = notificationTitle,
                Body = notificationBody,
                Type = notificationType,
                RelatedRequestId = linkedRequestId
            };
        }

        private static Notification BuildNotificationDomainModel(
            int recipientUserId,
            DateTime notificationTimestamp,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType = default,
            int? linkedRequestId = null)
        {
            return new Notification
            {
                Id = NewNotificationId,
                User = new User { Id = recipientUserId },
                Timestamp = notificationTimestamp,
                Title = notificationTitle,
                Body = notificationBody,
                Type = notificationType,
                RelatedRequestId = linkedRequestId
            };
        }

        private sealed class ScheduledReminder : IDisposable
        {
            private bool isDisposed;

            public ScheduledReminder(CancellationTokenSource cancellationSource)
            {
                CancellationSource = cancellationSource;
            }

            public CancellationTokenSource CancellationSource { get; }

            public Task? RunningTask { get; set; }

            public void Cancel()
            {
                try
                {
                    if (!CancellationSource.IsCancellationRequested)
                    {
                        CancellationSource.Cancel();
                    }
                }
                catch (ObjectDisposedException)
                {
                }
            }

            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
                CancellationSource.Dispose();
            }
        }
    }
}
