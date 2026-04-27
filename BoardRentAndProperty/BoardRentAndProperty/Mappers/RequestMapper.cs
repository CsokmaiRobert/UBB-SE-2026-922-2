using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class RequestMapper : IMapper<Request, RequestDTO>
    {
        private readonly IMapper<Game, GameDTO> requestedGameMapper;
        private readonly IMapper<User, UserDTO> requestParticipantUserMapper;

        public RequestMapper(IMapper<Game, GameDTO> gameMapper, IMapper<User, UserDTO> userMapper)
        {
            this.requestedGameMapper = gameMapper;
            this.requestParticipantUserMapper = userMapper;
        }

        public RequestDTO ToDTO(Request requestModel)
        {
            if (requestModel == null)
            {
                return null;
            }

            return new RequestDTO
            {
                Id = requestModel.Id,
                Game = requestedGameMapper.ToDTO(requestModel.Game),
                Renter = requestParticipantUserMapper.ToDTO(requestModel.Renter),
                Owner = requestParticipantUserMapper.ToDTO(requestModel.Owner),
                StartDate = requestModel.StartDate,
                EndDate = requestModel.EndDate,
                Status = requestModel.Status,
                OfferingUser = requestModel.OfferingUser != null ? requestParticipantUserMapper.ToDTO(requestModel.OfferingUser) : null
            };
        }

        public Request ToModel(RequestDTO requestDto)
        {
            if (requestDto == null)
            {
                return null;
            }

            return new Request
            {
                Id = requestDto.Id,
                Game = requestedGameMapper.ToModel(requestDto.Game),
                Renter = requestParticipantUserMapper.ToModel(requestDto.Renter),
                Owner = requestParticipantUserMapper.ToModel(requestDto.Owner),
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = requestDto.Status,
                OfferingUser = requestDto.OfferingUser != null ? requestParticipantUserMapper.ToModel(requestDto.OfferingUser) : null
            };
        }
    }
}