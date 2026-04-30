using System;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class RequestMapper : IMapper<Request, RequestDTO, int>
    {
        private readonly IMapper<Game, GameDTO, int> gameMapper;
        private readonly IMapper<Account, UserDTO, Guid> participantMapper;

        public RequestMapper(IMapper<Game, GameDTO, int> gameMapper, IMapper<Account, UserDTO, Guid> participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RequestDTO ToDTO(Request model)
        {
            if (model == null)
            {
                return null;
            }
            return new RequestDTO
            {
                Id = model.Id,
                Game = gameMapper.ToDTO(model.Game),
                Renter = participantMapper.ToDTO(model.Renter),
                Owner = participantMapper.ToDTO(model.Owner),
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                OfferingUser = model.OfferingUser != null ? participantMapper.ToDTO(model.OfferingUser) : null
            };
        }

        public Request ToModel(RequestDTO dto)
        {
            if (dto == null)
            {
                return null;
            }
            var renter = participantMapper.ToModel(dto.Renter);
            var owner = participantMapper.ToModel(dto.Owner);
            var offering = dto.OfferingUser != null ? participantMapper.ToModel(dto.OfferingUser) : null;
            return new Request
            {
                Id = dto.Id,
                Game = gameMapper.ToModel(dto.Game),
                Renter = renter,
                Owner = owner,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                OfferingUser = offering
            };
        }
    }
}
