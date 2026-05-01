using System;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class GameMapper : IMapper<Game, GameDTO, int>
    {
        private readonly IMapper<Account, UserDTO, Guid> ownerMapper;

        public GameMapper(IMapper<Account, UserDTO, Guid> ownerMapper)
        {
            this.ownerMapper = ownerMapper;
        }

        public GameDTO ToDTO(Game game)
        {
            if (game == null)
            {
                return null;
            }
            return new GameDTO
            {
                Id = game.Id,
                Owner = ownerMapper.ToDTO(game.Owner),
                Name = game.Name,
                Price = game.Price,
                MinimumPlayerNumber = game.MinimumPlayerNumber,
                MaximumPlayerNumber = game.MaximumPlayerNumber,
                Description = game.Description,
                Image = game.Image,
                IsActive = game.IsActive
            };
        }

        public Game ToModel(GameDTO dto)
        {
            if (dto == null)
            {
                return null;
            }
            var owner = ownerMapper.ToModel(dto.Owner);
            return new Game
            {
                Id = dto.Id,
                Owner = owner,
                Name = dto.Name,
                Price = dto.Price,
                MinimumPlayerNumber = dto.MinimumPlayerNumber,
                MaximumPlayerNumber = dto.MaximumPlayerNumber,
                Description = dto.Description,
                Image = dto.Image,
                IsActive = dto.IsActive
            };
        }
    }
}
