using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class GameMapper : IMapper<Game, GameDTO>
    {
        private readonly IMapper<User, UserDTO> gameOwnerUserMapper;

        public GameMapper(IMapper<User, UserDTO> gameOwnerMapper)
        {
            this.gameOwnerUserMapper = gameOwnerMapper;
        }

        public GameDTO ToDTO(Game gameModel)
        {
            if (gameModel == null)
            {
                return null;
            }

            return new GameDTO
            {
                Id = gameModel.Id,
                Owner = gameOwnerUserMapper.ToDTO(gameModel.Owner),
                Name = gameModel.Name,
                Price = gameModel.Price,
                MinimumPlayerNumber = gameModel.MinimumPlayerNumber,
                MaximumPlayerNumber = gameModel.MaximumPlayerNumber,
                Description = gameModel.Description,
                Image = gameModel.Image,
                IsActive = gameModel.IsActive
            };
        }

        public Game ToModel(GameDTO gameDto)
        {
            if (gameDto == null)
            {
                return null;
            }

            return new Game
            {
                Id = gameDto.Id,
                Owner = gameOwnerUserMapper.ToModel(gameDto.Owner),
                Name = gameDto.Name,
                Price = gameDto.Price,
                MinimumPlayerNumber = gameDto.MinimumPlayerNumber,
                MaximumPlayerNumber = gameDto.MaximumPlayerNumber,
                Description = gameDto.Description,
                Image = gameDto.Image,
                IsActive = gameDto.IsActive
            };
        }
    }
}