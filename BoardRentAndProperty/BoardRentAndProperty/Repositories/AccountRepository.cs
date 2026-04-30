namespace BoardRentAndProperty.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.Models;
    using Microsoft.EntityFrameworkCore;

    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext dbContext;

        public AccountRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Account> GetByIdAsync(Guid id)
        {
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Id == id);
        }

        public async Task<Account> GetByUsernameAsync(string username)
        {
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Username == username);
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            return await dbContext.Accounts.Include(account => account.Roles)
                .FirstOrDefaultAsync(account => account.Email == email);
        }

        public async Task<List<Account>> GetAllAsync(int page, int pageSize)
        {
            const int pageOffset = 1;
            return await dbContext.Accounts
                .Include(account => account.Roles)
                .OrderBy(account => account.CreatedAt)
                .Skip((page - pageOffset) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            dbContext.Accounts.Update(account);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddRoleAsync(Guid accountId, string roleName)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return;
            }

            bool alreadyHasRole = await dbContext.Set<AccountRole>().AnyAsync(ar => ar.AccountId == accountId && ar.RoleId == role.Id);
            if (!alreadyHasRole)
            {
                dbContext.Set<AccountRole>().Add(new AccountRole { AccountId = accountId, RoleId = role.Id });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
