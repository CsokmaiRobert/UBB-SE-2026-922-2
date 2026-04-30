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
using BoardRentAndProperty.Utilities;
namespace BoardRentAndProperty.Services
{
    public class NotificationService : INotificationService, IObserver<IncomingNotification>, IObservable<NotificationDTO>, IDisposable
    {
        private static readonly TimeSpan UpcomingRentalReminderLeadTime = TimeSpan.FromHours(24);
        private const int NewNotificationId = 0;
        private bool isDisposed;
        private readonly CancellationTokenSource reminderScheduleCancellationSource = new();
        private readonly INotificationRepository notificationDataRepository;
        private readonly IMapper<Notification, NotificationDTO, int> notificationDtoMapper;
        private readonly IServerClient serverNotificationClient;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IToastNotificationService toastAlertService;
        private readonly List<IObserver<NotificationDTO>> notificationSubscribers = new();
        private readonly object notificationSubscribersLock = new();

        public NotificationService(INotificationRepository notificationRepository, IMapper<Notification, NotificationDTO, int> notificationMapper,
            IServerClient serverClient, ICurrentUserContext currentUserContext, IToastNotificationService toastNotificationService)
        {
            this.notificationDataRepository = notificationRepository;
            this.notificationDtoMapper = notificationMapper;
            this.serverNotificationClient = serverClient;
            this.currentUserContext = currentUserContext;
            this.toastAlertService = toastNotificationService;
            serverNotificationClient.Subscribe(this);
        }

        private static int ToServerInt(Guid id) => Math.Abs(id.GetHashCode());

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Delete(notificationId));

        public NotificationDTO GetNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Get(notificationId));

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) =>
            notificationDataRepository.GetNotificationsByUser(accountId)
                .Select(n => notificationDtoMapper.ToDTO(n)).ToImmutableList();

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationToSend)
        {
            if (notificationToSend == null)
            {
                throw new ArgumentNullException(nameof(notificationToSend));
            }
            DateTime ts = notificationToSend.Timestamp == default ? DateTime.UtcNow : notificationToSend.Timestamp;
            var model = new Notification
            {
                Id = NewNotificationId,
                Recipient = new Account { Id = recipientAccountId },
                Timestamp = ts,
                Title = notificationToSend.Title,
                Body = notificationToSend.Body,
                Type = notificationToSend.Type,
                RelatedRequest = notificationToSend.RelatedRequestId.HasValue ? new Request { Id = notificationToSend.RelatedRequestId.Value } : null
            };
            notificationDataRepository.Add(model);
            if (currentUserContext.CurrentUserId == recipientAccountId)
            {
                NotifyAllSubscribers(new NotificationDTO
                {
                    Id = model.Id, User = new UserDTO { Id = recipientAccountId },
                    Timestamp = ts, Title = notificationToSend.Title, Body = notificationToSend.Body,
                    Type = notificationToSend.Type, RelatedRequestId = notificationToSend.RelatedRequestId
                });
            }
            serverNotificationClient.SendNotification(ToServerInt(recipientAccountId), notificationToSend.Title, notificationToSend.Body);
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId) =>
            notificationDataRepository.DeleteNotificationsLinkedToRequest(linkedRequestId);

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedDto) =>
            notificationDataRepository.Update(notificationId, notificationDtoMapper.ToModel(updatedDto));

        public void StartListening() =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await serverNotificationClient.ListenAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NotificationService: listen loop terminated - {ex}");
                }
            });

        public void StopListening() => serverNotificationClient.StopListening();
        public void OnCompleted()
        {
        }
        public void OnError(Exception error)
        {
        }

        public void OnNext(IncomingNotification received)
        {
            NotifyAllSubscribers(new NotificationDTO
            {
                Id = NewNotificationId, User = new UserDTO { Id = Guid.Empty },
                Timestamp = received.Timestamp, Title = received.Title, Body = received.Body
            });
            toastAlertService.Show(received.Title, received.Body);
        }

        private void NotifyAllSubscribers(NotificationDTO dto)
        {
            IObserver<NotificationDTO>[] snapshot;
            lock (notificationSubscribersLock)
            {
                snapshot = notificationSubscribers.ToArray();
            }
            foreach (var s in snapshot)
            {
                s.OnNext(dto);
            }
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer)
        {
            lock (notificationSubscribersLock)
            {
                notificationSubscribers.Add(observer);
            }
            return new Unsubscriber(notificationSubscribers, notificationSubscribersLock, observer);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> list;
            private readonly object listLock;
            private readonly IObserver<NotificationDTO> observer;
            public Unsubscriber(List<IObserver<NotificationDTO>> list, object lk, IObserver<NotificationDTO> obs)
            {
                this.list = list;
                listLock = lk;
                observer = obs;
            }
            public void Dispose()
            {
                lock (listLock)
                {
                    list.Remove(observer);
                }
            }
        }

        public void SubscribeToServer(Guid accountId) => serverNotificationClient.SubscribeToServer(ToServerInt(accountId));

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            reminderScheduleCancellationSource.Cancel();
            reminderScheduleCancellationSource.Dispose();
            StopListening();
            (serverNotificationClient as IDisposable)?.Dispose();
        }

        public void ScheduleUpcomingRentalReminder(Guid renterAccountId, Guid ownerAccountId, string gameName, DateTime rentalStartDate)
        {
            var rentalStartUtc = rentalStartDate.ToUniversalTime();
            if (rentalStartUtc <= DateTime.UtcNow)
            {
                return;
            }
            DateTime scheduledTime = rentalStartUtc - UpcomingRentalReminderLeadTime;
            string title = Constants.NotificationTitles.UpcomingRentalReminder;
            string body = $"Game: {gameName}\nStart: {rentalStartDate:dd/MM/yyyy HH:mm}\nDelivery/Pick-up: Coordinate delivery/pick-up directly with the other party.";
            ScheduleOrSendForUser(renterAccountId, title, body, scheduledTime);
            ScheduleOrSendForUser(ownerAccountId, title, body, scheduledTime);
        }

        private void ScheduleOrSendForUser(Guid accountId, string title, string body, DateTime scheduledTime)
        {
            if (accountId == Guid.Empty)
            {
                return;
            }
            TimeSpan delay = scheduledTime.ToUniversalTime() - DateTime.UtcNow;
            if (delay <= TimeSpan.Zero)
            {
                SendImmediate(accountId, title, body);
                return;
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, reminderScheduleCancellationSource.Token);
                    SendImmediate(accountId, title, body);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NotificationService: scheduled reminder failed - {ex}");
                }
            });
        }

        private void SendImmediate(Guid accountId, string title, string body)
        {
            SendNotificationToUser(accountId, new NotificationDTO { Id = NewNotificationId, User = new UserDTO { Id = accountId }, Timestamp = DateTime.UtcNow, Title = title, Body = body });
        }
    }
}
