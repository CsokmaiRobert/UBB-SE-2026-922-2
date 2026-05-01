using System;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IServerClient : IObservable<IncomingNotification>
    {
        event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        NotificationConnectionStatus ConnectionStatus { get; }

        Task ListenAsync();

        void SubscribeToServer(int targetUserId);

        void SendNotification(int targetUserId, string notificationTitle, string notificationBody);

        void StopListening();
    }
}
