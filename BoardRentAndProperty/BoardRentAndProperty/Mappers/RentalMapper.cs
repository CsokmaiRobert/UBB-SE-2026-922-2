using System;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class RentalMapper : IMapper<Rental, RentalDTO, int>
    {
        private readonly IMapper<Game, GameDTO, int> gameMapper;
        private readonly IMapper<Account, UserDTO, Guid> participantMapper;

        public RentalMapper(IMapper<Game, GameDTO, int> gameMapper, IMapper<Account, UserDTO, Guid> participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RentalDTO ToDTO(Rental model)
        {
            if (model == null)
            {
                return null;
            }
            return new RentalDTO
            {
                Id = model.Id,
                Game = gameMapper.ToDTO(model.Game),
                Renter = participantMapper.ToDTO(model.Renter),
                Owner = participantMapper.ToDTO(model.Owner),
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
        }

        public Rental ToModel(RentalDTO dto)
        {
            if (dto == null)
            {
                return null;
            }
            var renter = participantMapper.ToModel(dto.Renter);
            var owner = participantMapper.ToModel(dto.Owner);
            return new Rental
            {
                Id = dto.Id,
                Game = gameMapper.ToModel(dto.Game),
                Renter = renter,
                Owner = owner,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
        }
    }
}
