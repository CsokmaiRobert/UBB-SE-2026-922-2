using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class RequestMapper : IMapper<Request, RequestDTO>
    {
        private readonly IMapper<Game, GameDTO> requestedGameMapper;

        public RequestMapper(IMapper<Game, GameDTO> gameMapper)
        {
            this.requestedGameMapper = gameMapper;
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
                Renter = requestModel.Renter,
                Owner = requestModel.Owner,
                StartDate = requestModel.StartDate,
                EndDate = requestModel.EndDate,
                Status = requestModel.Status,
                OfferingAccount = requestModel.OfferingAccount
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
                Renter = requestDto.Renter,
                Owner = requestDto.Owner,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = requestDto.Status,
                OfferingAccount = requestDto.OfferingAccount
            };
        }
    }
}