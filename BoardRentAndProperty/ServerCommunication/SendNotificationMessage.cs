using System;

namespace ServerCommunication
{
    public class SendNotificationMessage : MessageBase
    {
        public int UserId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}
