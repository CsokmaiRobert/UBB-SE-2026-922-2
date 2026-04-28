using System;
using System.Threading.Tasks;
using BoardRentAndProperty.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public interface IServerClient : IObservable<IncomingNotification>
    {
        Task ListenAsync();
        void SubscribeToServer(int targetUserId);
        void SendNotification(int targetUserId, string notificationTitle, string notificationBody);
        void StopListening();
    }
}