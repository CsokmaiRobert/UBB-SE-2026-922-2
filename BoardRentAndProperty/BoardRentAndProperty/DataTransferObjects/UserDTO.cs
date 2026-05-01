using System;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
namespace BoardRentAndProperty.DataTransferObjects
{
    public class UserDTO : IDTO<Account, Guid>
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public UserDTO()
        {
        }
    }
}
