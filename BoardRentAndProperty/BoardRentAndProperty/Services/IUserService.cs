using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
namespace BoardRentAndProperty.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
