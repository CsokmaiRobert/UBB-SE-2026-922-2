namespace BoardRentAndProperty.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.Models;

    public interface IFailedLoginRepository
    {
        void SetUnitOfWork(IUnitOfWork unitOfWork);
        Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId);
        Task IncrementAsync(Guid accountId);
        Task ResetAsync(Guid accountId);
    }
}
