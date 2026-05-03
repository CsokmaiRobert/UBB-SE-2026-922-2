using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public AccountRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<Account?> GetByIdAsync(Guid id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Id == id);
        }

        public async Task<Account?> GetByUsernameAsync(string username)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Username == username);
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Email == email);
        }

        public async Task<List<Account>> GetAllAsync(int page, int pageSize)
        {
            const int pageOffset = 1;

            using var dbContext = this.dbContextFactory.CreateDbContext();
            return await dbContext.Accounts
                .Include(account => account.Roles)
                .OrderBy(account => account.CreatedAt)
                .Skip((page - pageOffset) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = await dbContext.Accounts.FindAsync(account.Id);
            if (existing == null)
            {
                return;
            }

            existing.DisplayName = account.DisplayName;
            existing.Username = account.Username;
            existing.Email = account.Email;
            existing.PasswordHash = account.PasswordHash;
            existing.PhoneNumber = account.PhoneNumber;
            existing.AvatarUrl = account.AvatarUrl;
            existing.IsSuspended = account.IsSuspended;
            existing.CreatedAt = account.CreatedAt;
            existing.UpdatedAt = account.UpdatedAt;
            existing.Country = account.Country;
            existing.City = account.City;
            existing.StreetName = account.StreetName;
            existing.StreetNumber = account.StreetNumber;

            await dbContext.SaveChangesAsync();
        }

        public async Task AddRoleAsync(Guid accountId, string roleName)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            var role = await dbContext.Roles.FirstOrDefaultAsync(repositoryRole => repositoryRole.Name == roleName);
            if (role == null)
            {
                return;
            }

            bool alreadyHasRole = await dbContext.Set<AccountRole>()
                .AnyAsync(accountRole => accountRole.AccountId == accountId && accountRole.RoleId == role.Id);

            if (!alreadyHasRole)
            {
                dbContext.Set<AccountRole>().Add(new AccountRole { AccountId = accountId, RoleId = role.Id });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
