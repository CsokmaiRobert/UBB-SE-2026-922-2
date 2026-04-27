using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userDataRepository;
        private readonly IMapper<User, UserDTO> userDtoMapper;

        public UserService(IUserRepository userRepository, IMapper<User, UserDTO> userMapper)
        {
            this.userDataRepository = userRepository;
            this.userDtoMapper = userMapper;
        }

        public ImmutableList<UserDTO> GetUsersExcept(int excludedUserId) =>
            userDataRepository.GetAll()
                .Where(user => user.Id != excludedUserId)
                .Select(user => userDtoMapper.ToDTO(user))
                .ToImmutableList();
    }
}