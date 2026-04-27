namespace BoardRent.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.Domain;

    public interface IFailedLoginRepository
    {
        void SetUnitOfWork(IUnitOfWork unitOfWork);
        Task<FailedLoginAttempt?> GetByUserIdAsync(Guid userId);
        Task IncrementAsync(Guid userId);
        Task ResetAsync(Guid userId);
    }
}