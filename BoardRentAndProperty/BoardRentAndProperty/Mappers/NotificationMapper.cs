using System;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class NotificationMapper : IMapper<Notification, NotificationDTO, int>
    {
        private readonly IMapper<Account, UserDTO, Guid> recipientMapper;

        public NotificationMapper(IMapper<Account, UserDTO, Guid> recipientMapper)
        {
            this.recipientMapper = recipientMapper;
        }

        public NotificationDTO ToDTO(Notification model)
        {
            if (model == null)
            {
                return null;
            }
            return new NotificationDTO
            {
                Id = model.Id,
                Recipient = recipientMapper.ToDTO(model.Recipient),
                Timestamp = model.Timestamp,
                Title = model.Title,
                Body = model.Body,
                Type = model.Type,
                RelatedRequestId = model.RelatedRequest?.Id
            };
        }

        public Notification ToModel(NotificationDTO dto)
        {
            if (dto == null)
            {
                return null;
            }
            var recipient = recipientMapper.ToModel(dto.Recipient);
            return new Notification
            {
                Id = dto.Id,
                Recipient = recipient,
                Timestamp = dto.Timestamp,
                Title = dto.Title,
                Body = dto.Body,
                Type = dto.Type,
                RelatedRequest = dto.RelatedRequestId.HasValue ? new Request { Id = dto.RelatedRequestId.Value } : null
            };
        }
    }
}
