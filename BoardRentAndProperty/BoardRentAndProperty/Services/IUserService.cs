using System;
using System.Collections.Immutable;
using BoardRentAndProperty.DataTransferObjects;
namespace BoardRentAndProperty.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
