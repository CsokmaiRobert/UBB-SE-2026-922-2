using System;
using BoardRentAndProperty.Contracts.Models;

namespace BoardRentAndProperty.Api.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public Account? Recipient { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Informational;
        public Request? RelatedRequest { get; set; }

        public Notification()
        {
        }

        public Notification(int id, Account? recipientAccount, DateTime timestamp, string title, string body,
                            NotificationType notificationType = NotificationType.Informational, Request? relatedRequest = null)
        {
            this.Id = id;
            Recipient = recipientAccount;
            Timestamp = timestamp;
            Title = title;
            Body = body;
            Type = notificationType;
            RelatedRequest = relatedRequest;
        }
    }
}
