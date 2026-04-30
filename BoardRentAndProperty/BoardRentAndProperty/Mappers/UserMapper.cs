using System;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;
namespace BoardRentAndProperty.Mappers
{
    public class UserMapper : IMapper<Account, UserDTO, Guid>
    {
        public UserDTO ToDTO(Account account)
        {
            if (account == null)
            {
                return null;
            }
            return new UserDTO
            {
                Id = account.Id,
                DisplayName = account.DisplayName
            };
        }
        public Account ToModel(UserDTO dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Account { Id = dto.Id, DisplayName = dto.DisplayName };
        }
    }
}
