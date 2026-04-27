using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.DataTransferObjects
{
    public class UserDTO : IDTO<User>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public UserDTO()
        {
        }
    }
}