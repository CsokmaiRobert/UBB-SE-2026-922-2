using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Api.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
