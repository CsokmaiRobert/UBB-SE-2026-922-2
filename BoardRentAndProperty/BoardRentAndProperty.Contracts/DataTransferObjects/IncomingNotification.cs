using System;

namespace BoardRentAndProperty.Contracts.DataTransferObjects
{
    public sealed class IncomingNotification
    {
        public int UserId { get; init; }
        public DateTime Timestamp { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }
}
