using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using ServerCommunication;

namespace BoardRentAndProperty.Services.Listeners
{
    public class NotificationClient : IServerClient, IDisposable
    {
        private const int DefaultNotificationServerPort = 4544;
        private const int AutoAssignLocalUdpPort = 0;
        private const int InitialRetryCount = 0;
        private const int RetryBackoffMultiplier = 2;
        private const int DefaultMaxRetries = 5;

        private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DefaultSubscriptionRefreshInterval = TimeSpan.FromSeconds(5);

        private readonly object clientStateLock = new();
        private readonly List<IObserver<IncomingNotification>> incomingNotificationSubscribers = new();
        private readonly int notificationServerPort;
        private readonly int maxRetries;
        private readonly TimeSpan subscriptionRefreshInterval;

        private bool isDisposed;
        private int? subscribedUserId;
        private DateTime? lastServerAcknowledgementUtc;
        private ListeningSession? activeListeningSession;
        private NotificationConnectionStatus connectionStatus = NotificationConnectionStatus.Stopped;

        public event EventHandler<NotificationConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public NotificationConnectionStatus ConnectionStatus
        {
            get
            {
                lock (clientStateLock)
                {
                    return connectionStatus;
                }
            }
        }

        public IPEndPoint ServerEndpoint => new IPEndPoint(IPAddress.Loopback, notificationServerPort);

        public NotificationClient()
            : this(DefaultNotificationServerPort, DefaultMaxRetries, DefaultSubscriptionRefreshInterval)
        {
        }

        internal NotificationClient(int notificationServerPort, int maxRetries, TimeSpan subscriptionRefreshInterval)
        {
            this.notificationServerPort = notificationServerPort;
            this.maxRetries = maxRetries;
            this.subscriptionRefreshInterval = subscriptionRefreshInterval;
        }

        private void HandleMessagePacket(MessageWrapper wrappedMessage)
        {
            try
            {
                switch (wrappedMessage.Type)
                {
                    case nameof(ServerStatusMessage):
                        break;
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

            foreach (var subscriber in GetSubscribersSnapshot())
            {
                subscriber.OnNext(incomingNotification);
            }
        }

        public void StopListening()
        {
            ListeningSession? sessionToStop;
            lock (clientStateLock)
            {
                sessionToStop = activeListeningSession;
                activeListeningSession = null;
            }

            sessionToStop?.Cancel();
            SetConnectionStatus(NotificationConnectionStatus.Stopped);
        }

        public Task ListenAsync()
        {
            ThrowIfDisposed();

            ListeningSession listeningSession;
            lock (clientStateLock)
            {
                if (activeListeningSession?.ListeningTask is { IsCompleted: false } runningListeningTask)
                {
                    return runningListeningTask;
                }

                listeningSession = new ListeningSession(new UdpClient(AutoAssignLocalUdpPort), new CancellationTokenSource());
                listeningSession.ListeningTask = RunListeningSessionAsync(listeningSession);
                activeListeningSession = listeningSession;
            }

            SetConnectionStatus(NotificationConnectionStatus.Connected);
            return listeningSession.ListeningTask;
        }

        private async Task RunListeningSessionAsync(ListeningSession listeningSession)
        {
            var subscriptionTask = RefreshSubscriptionAsync(listeningSession);
            var currentRetryCount = InitialRetryCount;
            var currentRetryDelay = InitialRetryDelay;

            try
            {
                while (!listeningSession.CancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var receivedResult = await listeningSession.UdpSocketClient.ReceiveAsync(listeningSession.CancellationToken);
                        currentRetryCount = InitialRetryCount;
                        currentRetryDelay = InitialRetryDelay;
                        HandleReceivedPayload(receivedResult.Buffer);
                    }
                    catch (SocketException socketException)
                    {
                        currentRetryCount++;
                        if (currentRetryCount > maxRetries)
                        {
                            System.Diagnostics.Debug.WriteLine($"UDP client stopped after retry limit. Last error: {socketException.Message}");
                            SetConnectionStatus(NotificationConnectionStatus.Offline);
                            break;
                        }

                        SetConnectionStatus(NotificationConnectionStatus.Reconnecting);
                        await Task.Delay(currentRetryDelay, listeningSession.CancellationToken);
                        currentRetryDelay = TimeSpan.FromTicks(Math.Min(currentRetryDelay.Ticks * RetryBackoffMultiplier, MaxRetryDelay.Ticks));
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                listeningSession.Cancel();
                await WaitForSubscriptionTaskAsync(subscriptionTask);
                CompleteListeningSession(listeningSession);
            }
        }

        private void HandleReceivedPayload(byte[] receivedPayload)
        {
            MessageWrapper? wrappedMessage = CommunicationHelper.GetMessageWrapper(receivedPayload);

            if (wrappedMessage == null)
            {
                System.Diagnostics.Debug.WriteLine($"Received bad json: {Encoding.UTF8.GetString(receivedPayload)}");
                return;
            }

            MarkServerAcknowledged();
            HandleMessagePacket(wrappedMessage);
        }

        private async Task RefreshSubscriptionAsync(ListeningSession listeningSession)
        {
            var missedAcknowledgementCount = InitialRetryCount;
            var currentRetryDelay = InitialRetryDelay;

            while (!listeningSession.CancellationToken.IsCancellationRequested)
            {
                var targetUserId = GetSubscribedUserId();
                if (!targetUserId.HasValue)
                {
                    if (!await DelaySubscriptionRefreshAsync(subscriptionRefreshInterval, listeningSession.CancellationToken))
                    {
                        break;
                    }

                    continue;
                }

                DateTime subscriptionSentAtUtc = DateTime.UtcNow;
                SendMessageThroughSession(
                    listeningSession,
                    new SubscribeToServerMessage { UserId = targetUserId.Value });

                var nextRefreshDelay = missedAcknowledgementCount == InitialRetryCount
                    ? subscriptionRefreshInterval
                    : currentRetryDelay;

                if (!await DelaySubscriptionRefreshAsync(nextRefreshDelay, listeningSession.CancellationToken))
                {
                    break;
                }

                if (HasServerAcknowledgedSince(subscriptionSentAtUtc))
                {
                    missedAcknowledgementCount = InitialRetryCount;
                    currentRetryDelay = InitialRetryDelay;
                    SetConnectionStatus(NotificationConnectionStatus.Connected);
                    continue;
                }

                missedAcknowledgementCount++;
                if (missedAcknowledgementCount > maxRetries)
                {
                    SetConnectionStatus(NotificationConnectionStatus.Offline);
                }
                else
                {
                    SetConnectionStatus(NotificationConnectionStatus.Reconnecting);
                }

                currentRetryDelay = TimeSpan.FromTicks(Math.Min(currentRetryDelay.Ticks * RetryBackoffMultiplier, MaxRetryDelay.Ticks));
            }
        }

        private static async Task<bool> DelaySubscriptionRefreshAsync(TimeSpan refreshDelay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(refreshDelay, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private static async Task WaitForSubscriptionTaskAsync(Task subscriptionTask)
        {
            try
            {
                await subscriptionTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void CompleteListeningSession(ListeningSession completedListeningSession)
        {
            var shouldSetStoppedStatus = false;
            lock (clientStateLock)
            {
                if (ReferenceEquals(activeListeningSession, completedListeningSession))
                {
                    activeListeningSession = null;
                    if (connectionStatus != NotificationConnectionStatus.Offline)
                    {
                        shouldSetStoppedStatus = true;
                    }
                }
            }

            completedListeningSession.Dispose();
            if (shouldSetStoppedStatus)
            {
                SetConnectionStatus(NotificationConnectionStatus.Stopped);
            }
        }

        public IDisposable Subscribe(IObserver<IncomingNotification> newObserver)
        {
            lock (incomingNotificationSubscribers)
            {
                incomingNotificationSubscribers.Add(newObserver);
            }

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

            SendMessage(outgoingNotificationMessage);
        }

        public void SubscribeToServer(int subscribingUserId)
        {
            lock (clientStateLock)
            {
                subscribedUserId = subscribingUserId;
            }

            SetConnectionStatus(NotificationConnectionStatus.Reconnecting);
            SendMessage(new SubscribeToServerMessage { UserId = subscribingUserId });
        }

        private void SendMessage(MessageBase messageToSend)
        {
            ThrowIfDisposed();

            var activeSession = GetActiveListeningSession();
            if (activeSession != null)
            {
                SendMessageThroughSession(activeSession, messageToSend);
                return;
            }

            try
            {
                using var transientUdpClient = new UdpClient(AutoAssignLocalUdpPort);
                SendMessageThroughClient(transientUdpClient, messageToSend);
            }
            catch (SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine($"UDP send failed: {socketException.Message}");
                SetConnectionStatus(NotificationConnectionStatus.Reconnecting);
            }
        }

        private void SendMessageThroughSession(ListeningSession listeningSession, MessageBase messageToSend)
        {
            try
            {
                SendMessageThroughClient(listeningSession.UdpSocketClient, messageToSend);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine($"UDP send failed: {socketException.Message}");
                SetConnectionStatus(NotificationConnectionStatus.Reconnecting);
            }
        }

        private void SendMessageThroughClient(UdpClient udpClient, MessageBase messageToSend)
        {
            byte[] serializedData = CommunicationHelper.SerializeMessage(messageToSend);
            udpClient.Send(serializedData, serializedData.Length, ServerEndpoint);
        }

        private ListeningSession? GetActiveListeningSession()
        {
            lock (clientStateLock)
            {
                if (activeListeningSession?.ListeningTask is { IsCompleted: false })
                {
                    return activeListeningSession;
                }

                return null;
            }
        }

        private int? GetSubscribedUserId()
        {
            lock (clientStateLock)
            {
                return subscribedUserId;
            }
        }

        private void MarkServerAcknowledged()
        {
            lock (clientStateLock)
            {
                lastServerAcknowledgementUtc = DateTime.UtcNow;
            }

            SetConnectionStatus(NotificationConnectionStatus.Connected);
        }

        private bool HasServerAcknowledgedSince(DateTime sentAtUtc)
        {
            lock (clientStateLock)
            {
                return lastServerAcknowledgementUtc.HasValue
                       && lastServerAcknowledgementUtc.Value >= sentAtUtc;
            }
        }

        private IObserver<IncomingNotification>[] GetSubscribersSnapshot()
        {
            lock (incomingNotificationSubscribers)
            {
                return incomingNotificationSubscribers.ToArray();
            }
        }

        private void SetConnectionStatus(NotificationConnectionStatus updatedConnectionStatus)
        {
            EventHandler<NotificationConnectionStatusChangedEventArgs>? statusChangedHandler;
            lock (clientStateLock)
            {
                if (connectionStatus == updatedConnectionStatus)
                {
                    return;
                }

                connectionStatus = updatedConnectionStatus;
                statusChangedHandler = ConnectionStatusChanged;
            }

            statusChangedHandler?.Invoke(this, new NotificationConnectionStatusChangedEventArgs(updatedConnectionStatus));
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(NotificationClient));
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            StopListening();
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

            public void Dispose()
            {
                lock (subscribersList)
                {
                    subscribersList.Remove(subscriberToRemove);
                }
            }
        }

        private sealed class ListeningSession : IDisposable
        {
            private bool isDisposed;

            public ListeningSession(UdpClient udpSocketClient, CancellationTokenSource cancellationSource)
            {
                UdpSocketClient = udpSocketClient;
                CancellationSource = cancellationSource;
            }

            public UdpClient UdpSocketClient { get; }

            public CancellationTokenSource CancellationSource { get; }

            public CancellationToken CancellationToken => CancellationSource.Token;

            public Task? ListeningTask { get; set; }

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

                UdpSocketClient.Close();
            }

            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
                UdpSocketClient.Dispose();
                CancellationSource.Dispose();
            }
        }
    }
}
