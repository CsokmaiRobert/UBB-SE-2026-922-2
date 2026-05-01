using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Mappers
{
    public class NotificationMapper
    {
        private readonly UserMapper recipientMapper;

        public NotificationMapper(UserMapper recipientMapper)
        {
            this.recipientMapper = recipientMapper;
        }

        public NotificationDTO? ToDTO(Notification? notification)
        {
            if (notification == null)
            {
                return null;
            }

            return new NotificationDTO
            {
                Id = notification.Id,
                Recipient = recipientMapper.ToDTO(notification.Recipient),
                Timestamp = notification.Timestamp,
                Title = notification.Title,
                Body = notification.Body,
                Type = notification.Type,
                RelatedRequestId = notification.RelatedRequest?.Id,
            };
        }

        public Notification? ToModel(NotificationDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Notification
            {
                Id = dto.Id,
                Recipient = recipientMapper.ToModel(dto.Recipient),
                Timestamp = dto.Timestamp,
                Title = dto.Title,
                Body = dto.Body,
                Type = dto.Type,
                RelatedRequest = dto.RelatedRequestId.HasValue ? new Request { Id = dto.RelatedRequestId.Value } : null,
            };
        }
    }
}
