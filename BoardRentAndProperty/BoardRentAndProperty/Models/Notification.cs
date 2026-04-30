using System;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public Account Recipient { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Informational;
        public int? RelatedRequestId { get; set; }

        public Notification()
        {
        }
        public Notification(int id, Account recipientAccount, DateTime timestamp, string title, string body,
                            NotificationType notificationType = NotificationType.Informational, int? relatedRequestId = null)
        {
            this.Id = id;
            Recipient = recipientAccount;
            Timestamp = timestamp;
            Title = title;
            Body = body;
            Type = notificationType;
            this.RelatedRequestId = relatedRequestId;
        }
    }
}