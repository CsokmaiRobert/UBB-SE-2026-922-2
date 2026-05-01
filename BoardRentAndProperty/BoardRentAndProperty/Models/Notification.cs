using System;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class Notification : IEntity<int>
    {
        public int Id { get; set; }
        public Account Recipient { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Informational;
        public Request? RelatedRequest { get; set; }

        public Notification()
        {
        }

        public Notification(int id, Account recipientAccount, DateTime timestamp, string title, string body,
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
