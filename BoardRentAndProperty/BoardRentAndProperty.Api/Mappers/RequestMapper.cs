using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Mappers
{
    public class RequestMapper
    {
        private readonly GameMapper gameMapper;
        private readonly UserMapper participantMapper;

        public RequestMapper(GameMapper gameMapper, UserMapper participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RequestDTO? ToDTO(Request? request)
        {
            if (request == null)
            {
                return null;
            }

            return new RequestDTO
            {
                Id = request.Id,
                Game = gameMapper.ToDTO(request.Game),
                Renter = participantMapper.ToDTO(request.Renter),
                Owner = participantMapper.ToDTO(request.Owner),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                OfferingUser = request.OfferingUser != null ? participantMapper.ToDTO(request.OfferingUser) : null,
            };
        }

        public Request? ToModel(RequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Request
            {
                Id = dto.Id,
                Game = gameMapper.ToModel(dto.Game),
                Renter = participantMapper.ToModel(dto.Renter),
                Owner = participantMapper.ToModel(dto.Owner),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                OfferingUser = dto.OfferingUser != null ? participantMapper.ToModel(dto.OfferingUser) : null,
            };
        }
    }
}
