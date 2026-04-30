namespace BoardRentAndProperty.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Models;

    public interface IFailedLoginRepository
    {
        Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId);
        Task IncrementAsync(Guid accountId);
        Task ResetAsync(Guid accountId);
    }
}
