using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Mappers
{
    public class GameMapper
    {
        private readonly UserMapper ownerMapper;

        public GameMapper(UserMapper ownerMapper)
        {
            this.ownerMapper = ownerMapper;
        }

        public GameDTO? ToDTO(Game? game)
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
                IsActive = game.IsActive,
            };
        }

        public Game? ToModel(GameDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Game
            {
                Id = dto.Id,
                Owner = ownerMapper.ToModel(dto.Owner),
                Name = dto.Name,
                Price = dto.Price,
                MinimumPlayerNumber = dto.MinimumPlayerNumber,
                MaximumPlayerNumber = dto.MaximumPlayerNumber,
                Description = dto.Description,
                Image = dto.Image,
                IsActive = dto.IsActive,
            };
        }
    }
}
