using System;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Repositories
{
    public class FailedLoginRepository : IFailedLoginRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public FailedLoginRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.FailedLoginAttempts.FirstOrDefaultAsync(failedLogin => failedLogin.AccountId == accountId);
        }

        public async Task IncrementAsync(Guid accountId)
        {
            const int lockThreshold = 5;
            const int lockMinutes = 15;

            using var dbContext = this.dbContextFactory.CreateDbContext();
            var attempt = await dbContext.FailedLoginAttempts.FirstOrDefaultAsync(failedLogin => failedLogin.AccountId == accountId);
            if (attempt == null)
            {
                dbContext.FailedLoginAttempts.Add(new FailedLoginAttempt { AccountId = accountId, FailedAttempts = 1, LockedUntil = null });
            }
            else
            {
                attempt.FailedAttempts++;
                if (attempt.FailedAttempts >= lockThreshold)
                {
                    attempt.LockedUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task ResetAsync(Guid accountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var attempt = await dbContext.FailedLoginAttempts.FirstOrDefaultAsync(failedLogin => failedLogin.AccountId == accountId);
            if (attempt != null)
            {
                attempt.FailedAttempts = 0;
                attempt.LockedUntil = null;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
