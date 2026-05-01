using System;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.DataTransferObjects
{
    public class NotificationDTO : IDTO<Notification, int>
    {
        private const string TimeDisplayFormat = "dd MMM yyyy, HH:mm";

        public int Id { get; set; }
        public UserDTO? Recipient { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Informational;
        public int? RelatedRequestId { get; set; }

        public string TimeDisplay => Timestamp.ToLocalTime().ToString(TimeDisplayFormat);

        public NotificationDTO()
        {
        }
    }
}
