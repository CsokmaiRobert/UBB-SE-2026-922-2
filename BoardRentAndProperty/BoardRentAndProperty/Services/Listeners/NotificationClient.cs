using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using ServerCommunication;

namespace BoardRentAndProperty.Services.Listeners
{
    public class NotificationClient : IServerClient, IDisposable
    {
        private const int NotificationServerPort = 4544;
        private const int AutoAssignLocalUdpPort = 0;
        private const int InitialRetryCount = 0;
        private const int RetryBackoffMultiplier = 2;
        private bool isDisposed;

        private readonly List<IObserver<IncomingNotification>> incomingNotificationSubscribers = new();
        private readonly UdpClient udpSocketClient;

        private readonly CancellationTokenSource listenCancellationSource = new();
        private CancellationToken ListenCancellationToken => listenCancellationSource.Token;

        private const int MaxRetries = 5;
        private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

        public IPEndPoint ServerEndpoint => new IPEndPoint(IPAddress.Loopback, NotificationServerPort);

        private NotificationConnectionStatus connectionStatus = NotificationConnectionStatus.Stopped;

        public NotificationConnectionStatus ConnectionStatus => connectionStatus;

        public event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public NotificationClient()
        {
            udpSocketClient = new UdpClient(AutoAssignLocalUdpPort);
        }

        [SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "A malformed notification packet should not stop the listener loop.")]
        private void HandleMessagePacket(MessageWrapper wrappedMessage)
        {
            try
            {
                switch (wrappedMessage.Type)
                {
                    case nameof(SendNotificationMessage):
                        HandleSendNotificationMessage(wrappedMessage);
                        break;
                    default:
                        Console.WriteLine($"Message type cannot be handled: {wrappedMessage.Type}");
                        break;
                }
            }
            catch (Exception messageHandlingException)
            {
                Console.WriteLine($"Exception when handling message packet: {messageHandlingException.Message}");
            }
        }

        private void HandleSendNotificationMessage(MessageWrapper wrappedMessage)
        {
            SendNotificationMessage? deserializedMessage = wrappedMessage.Deserialize<SendNotificationMessage>();

            if (deserializedMessage == null)
            {
                throw new ArgumentNullException(nameof(deserializedMessage));
            }

            var incomingNotification = new IncomingNotification
            {
                UserId = deserializedMessage.UserId,
                Timestamp = deserializedMessage.Timestamp,
                Title = deserializedMessage.Title,
                Body = deserializedMessage.Body
            };

            foreach (var subscriber in incomingNotificationSubscribers)
            {
                subscriber.OnNext(incomingNotification);
            }
        }

        public void StopListening() => listenCancellationSource.Cancel();

        public async Task ListenAsync()
        {
            int currentRetryCount = InitialRetryCount;
            var currentRetryDelay = InitialRetryDelay;
            // entering listen loop - attempt to connect
            UpdateConnectionStatus(NotificationConnectionStatus.Reconnecting);

            while (!ListenCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var receivedResult = await udpSocketClient.ReceiveAsync(ListenCancellationToken);
                    // successfully received data - consider connected
                    UpdateConnectionStatus(NotificationConnectionStatus.Connected);
                    currentRetryCount = InitialRetryCount;
                    currentRetryDelay = InitialRetryDelay;

                    MessageWrapper? wrappedMessage = CommunicationHelper.GetMessageWrapper(receivedResult.Buffer);

                    if (wrappedMessage == null)
                    {
                        Console.WriteLine($"Received bad json: {Encoding.UTF8.GetString(receivedResult.Buffer)}");
                        continue;
                    }

                    HandleMessagePacket(wrappedMessage);
                }
                catch (SocketException socketException)
                {
                    currentRetryCount++;
                    // indicate reconnecting while retrying
                    UpdateConnectionStatus(NotificationConnectionStatus.Reconnecting);

                    if (currentRetryCount > MaxRetries)
                    {
                        Console.WriteLine($"UDP client: max retries ({MaxRetries}) reached, stopping. Last error: {socketException.Message}");
                        UpdateConnectionStatus(NotificationConnectionStatus.Offline);
                        break;
                    }
                    Console.WriteLine($"UDP client: SocketException ({socketException.Message}), retry {currentRetryCount}/{MaxRetries} in {currentRetryDelay.TotalSeconds}s");
                    try
                    {
                        await Task.Delay(currentRetryDelay, ListenCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    currentRetryDelay = TimeSpan.FromTicks(Math.Min(currentRetryDelay.Ticks * RetryBackoffMultiplier, MaxRetryDelay.Ticks));
                }
                catch (OperationCanceledException)
                {
                    UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
                    break;
                }
                catch (ObjectDisposedException)
                {
                    UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
                    break;
                }
            }

            // loop finished - ensure stopped if not already offline
            if (ConnectionStatus != NotificationConnectionStatus.Offline)
            {
                UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
            }
        }

        public IDisposable Subscribe(IObserver<IncomingNotification> newObserver)
        {
            incomingNotificationSubscribers.Add(newObserver);
            return new Unsubscriber(incomingNotificationSubscribers, newObserver);
        }

        public void SendNotification(int recipientUserId, string notificationTitle, string notificationBody)
        {
            var outgoingNotificationMessage = new SendNotificationMessage
            {
                UserId = recipientUserId,
                Timestamp = DateTime.UtcNow,
                Title = notificationTitle,
                Body = notificationBody
            };

            byte[] serializedData = CommunicationHelper.SerializeMessage(outgoingNotificationMessage);
            udpSocketClient.Send(serializedData, serializedData.Length, ServerEndpoint);
        }

        public void SubscribeToServer(int subscribingUserId)
        {
            var subscriptionMessage = new SubscribeToServerMessage { UserId = subscribingUserId };
            byte[] serializedData = CommunicationHelper.SerializeMessage(subscriptionMessage);
            udpSocketClient.Send(serializedData, serializedData.Length, ServerEndpoint);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            listenCancellationSource.Cancel();
            udpSocketClient.Close();
            listenCancellationSource.Dispose();

            UpdateConnectionStatus(NotificationConnectionStatus.Stopped);
        }

        private void UpdateConnectionStatus(NotificationConnectionStatus newStatus)
        {
            if (connectionStatus == newStatus)
            {
                return;
            }

            connectionStatus = newStatus;
            ConnectionStatusChanged?.Invoke(this, new NotificationConnectionStatusChangedEventArgs(newStatus));
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<IncomingNotification>> subscribersList;
            private readonly IObserver<IncomingNotification> subscriberToRemove;

            public Unsubscriber(List<IObserver<IncomingNotification>> subscribers, IObserver<IncomingNotification> observer)
            {
                this.subscribersList = subscribers;
                this.subscriberToRemove = observer;
            }

            public void Dispose() => subscribersList.Remove(subscriberToRemove);
        }
    }
}
