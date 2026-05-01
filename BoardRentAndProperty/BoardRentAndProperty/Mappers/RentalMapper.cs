using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class RentalMapper : IMapper<Rental, RentalDTO>
    {
        private readonly IMapper<Game, GameDTO> rentalGameMapper;

        public RentalMapper(IMapper<Game, GameDTO> gameMapper)
        {
            this.rentalGameMapper = gameMapper;
        }

        public RentalDTO ToDTO(Rental rentalModel)
        {
            if (rentalModel == null)
            {
                return null;
            }

            return new RentalDTO
            {
                Id = rentalModel.Id,
                Game = rentalGameMapper.ToDTO(rentalModel.Game),
                Renter = rentalModel.Renter,
                Owner = rentalModel.Owner,
                StartDate = rentalModel.StartDate,
                EndDate = rentalModel.EndDate
            };
        }

        public Rental ToModel(RentalDTO rentalDto)
        {
            if (rentalDto == null)
            {
                return null;
            }

            return new Rental
            {
                Id = rentalDto.Id,
                Game = rentalGameMapper.ToModel(rentalDto.Game),
                Renter = rentalDto.Renter,
                Owner = rentalDto.Owner,
                StartDate = rentalDto.StartDate,
                EndDate = rentalDto.EndDate
            };
        }
    }
}