using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class UserMapper : IMapper<User, UserDTO>
    {
        public UserDTO ToDTO(User userModel)
        {
            if (userModel == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = userModel.Id,
                DisplayName = userModel.DisplayName
            };
        }

        public User ToModel(UserDTO userDto)
        {
            if (userDto == null)
            {
                return null;
            }

            return new User
            {
                Id = userDto.Id,
                DisplayName = userDto.DisplayName
            };
        }
    }
}