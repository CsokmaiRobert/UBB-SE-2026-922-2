using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Services
{
    public class NotificationService : INotificationService, IObserver<IncomingNotification>, IDisposable
    {
        private const int NewNotificationId = 0;

        private bool isDisposed;

        private readonly HttpClient httpClient;
        private readonly IServerClient serverNotificationClient;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IToastNotificationService toastAlertService;
        private readonly List<IObserver<NotificationDTO>> notificationSubscribers = new();
        private readonly object notificationSubscribersLock = new();

        public NotificationService(HttpClient httpClient,
                                   IServerClient serverClient,
                                   ICurrentUserContext currentUserContext,
                                   IToastNotificationService toastNotificationService)
        {
            this.httpClient = httpClient;
            this.serverNotificationClient = serverClient;
            this.currentUserContext = currentUserContext;
            this.toastAlertService = toastNotificationService;
            this.serverNotificationClient.Subscribe(this);
        }

        private static int ToServerInt(Guid id) => Math.Abs(id.GetHashCode());

        public NotificationDTO GetNotificationByIdentifier(int notificationId)
        {
            var response = this.httpClient.GetAsync($"api/notifications/{notificationId}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<NotificationDTO>().GetAwaiter().GetResult() ?? new NotificationDTO();
        }

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId)
        {
            var response = this.httpClient.DeleteAsync($"api/notifications/{notificationId}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<NotificationDTO>().GetAwaiter().GetResult() ?? new NotificationDTO { Id = notificationId };
        }

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedDto)
        {
            var response = this.httpClient.PutAsJsonAsync($"api/notifications/{notificationId}", updatedDto).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId)
        {
            try
            {
                var response = this.httpClient.GetAsync($"api/notifications/user/{accountId}").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    return ImmutableList<NotificationDTO>.Empty;
                }

                var list = response.Content.ReadFromJsonAsync<List<NotificationDTO>>().GetAwaiter().GetResult() ?? new List<NotificationDTO>();
                return list.ToImmutableList();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get notifications for user {accountId}: {ex.Message}");
                return ImmutableList<NotificationDTO>.Empty;
            }
        }

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationToSend)
        {
            if (notificationToSend == null)
            {
                throw new ArgumentNullException(nameof(notificationToSend));
            }

            DateTime timestamp = notificationToSend.Timestamp == default ? DateTime.UtcNow : notificationToSend.Timestamp;
            var payload = new NotificationDTO
            {
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = recipientAccountId },
                Timestamp = timestamp,
                Title = notificationToSend.Title,
                Body = notificationToSend.Body,
                Type = notificationToSend.Type,
                RelatedRequestId = notificationToSend.RelatedRequestId,
            };

            this.httpClient.PutAsJsonAsync($"api/notifications/0", payload).GetAwaiter().GetResult();

            if (this.currentUserContext.CurrentUserId == recipientAccountId)
            {
                NotifyAllSubscribers(payload);
                this.toastAlertService.Show(notificationToSend.Title, notificationToSend.Body);
                return;
            }

            this.serverNotificationClient.SendNotification(ToServerInt(recipientAccountId), notificationToSend.Title, notificationToSend.Body);
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId)
        {
            var response = this.httpClient.DeleteAsync($"api/notifications/request/{linkedRequestId}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void StartListening() =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await this.serverNotificationClient.ListenAsync();
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NotificationService: listen loop terminated due to socket exception - {ex}");
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NotificationService: listen loop terminated due to invalid operation - {ex}");
                }
            });

        public void StopListening() => this.serverNotificationClient.StopListening();

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
                Id = NewNotificationId,
                Recipient = new UserDTO { Id = Guid.Empty },
                Timestamp = received.Timestamp,
                Title = received.Title,
                Body = received.Body,
            });

            this.toastAlertService.Show(received.Title, received.Body);
        }

        private void NotifyAllSubscribers(NotificationDTO dto)
        {
            IObserver<NotificationDTO>[] snapshot;
            lock (this.notificationSubscribersLock)
            {
                snapshot = this.notificationSubscribers.ToArray();
            }

            foreach (var subscriber in snapshot)
            {
                subscriber.OnNext(dto);
            }
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer)
        {
            lock (this.notificationSubscribersLock)
            {
                this.notificationSubscribers.Add(observer);
            }

            return new Unsubscriber(this.notificationSubscribers, this.notificationSubscribersLock, observer);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> list;
            private readonly object listLock;
            private readonly IObserver<NotificationDTO> observer;

            public Unsubscriber(List<IObserver<NotificationDTO>> list, object listLock, IObserver<NotificationDTO> observer)
            {
                this.list = list;
                this.listLock = listLock;
                this.observer = observer;
            }

            public void Dispose()
            {
                lock (this.listLock)
                {
                    this.list.Remove(this.observer);
                }
            }
        }

        public void SubscribeToServer(Guid accountId) => this.serverNotificationClient.SubscribeToServer(ToServerInt(accountId));

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
    }
}
