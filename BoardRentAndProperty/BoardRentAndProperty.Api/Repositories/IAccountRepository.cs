using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Models;

namespace BoardRentAndProperty.Api.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id);
        Task<Account?> GetByUsernameAsync(string username);
        Task<Account?> GetByEmailAsync(string email);
        Task<List<Account>> GetAllAsync(int page, int pageSize);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task AddRoleAsync(Guid accountId, string roleName);
    }
}
