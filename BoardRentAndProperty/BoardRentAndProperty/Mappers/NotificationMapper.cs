using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class NotificationMapper : IMapper<Notification, NotificationDTO>
    {
        public NotificationDTO ToDTO(Notification notificationModel)
        {
            if (notificationModel == null)
            {
                return null;
            }

            return new NotificationDTO
            {
                Id = notificationModel.Id,
                Recipient = notificationModel.Recipient,
                Timestamp = notificationModel.Timestamp,
                Title = notificationModel.Title,
                Body = notificationModel.Body,
                Type = notificationModel.Type,
                RelatedRequestId = notificationModel.RelatedRequestId
            };
        }

        public Notification ToModel(NotificationDTO notificationDto)
        {
            if (notificationDto == null)
            {
                return null;
            }

            return new Notification
            {
                Id = notificationDto.Id,
                Recipient = notificationDto.Recipient,
                Timestamp = notificationDto.Timestamp,
                Title = notificationDto.Title,
                Body = notificationDto.Body,
                Type = notificationDto.Type,
                RelatedRequestId = notificationDto.RelatedRequestId
            };
        }
    }
}