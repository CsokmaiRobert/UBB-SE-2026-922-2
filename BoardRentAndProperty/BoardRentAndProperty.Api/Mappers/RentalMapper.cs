using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Mappers
{
    public class RentalMapper
    {
        private readonly GameMapper gameMapper;
        private readonly UserMapper participantMapper;

        public RentalMapper(GameMapper gameMapper, UserMapper participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RentalDTO? ToDTO(Rental? rental)
        {
            if (rental == null)
            {
                return null;
            }

            return new RentalDTO
            {
                Id = rental.Id,
                Game = gameMapper.ToDTO(rental.Game),
                Renter = participantMapper.ToDTO(rental.Renter),
                Owner = participantMapper.ToDTO(rental.Owner),
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
            };
        }

        public Rental? ToModel(RentalDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Rental
            {
                Id = dto.Id,
                Game = gameMapper.ToModel(dto.Game),
                Renter = participantMapper.ToModel(dto.Renter),
                Owner = participantMapper.ToModel(dto.Owner),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
            };
        }
    }
}
