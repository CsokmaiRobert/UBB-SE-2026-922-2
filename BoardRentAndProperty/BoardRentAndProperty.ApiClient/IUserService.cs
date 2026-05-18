using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.ApiClient
{
    public interface IUserService
    {
        Task<ServiceResult<IReadOnlyList<UserDTO>>> GetUsersExceptAsync(Guid excludeAccountId, CancellationToken cancellationToken = default);
    }
}
