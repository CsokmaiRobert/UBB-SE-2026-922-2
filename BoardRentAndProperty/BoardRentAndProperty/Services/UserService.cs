using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Models;
namespace BoardRentAndProperty.Services
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IMapper<Account, UserDTO, Guid> accountMapper;
        public UserService(IAccountRepository accountRepository, IMapper<Account, UserDTO, Guid> accountMapper)
        {
            this.accountRepository = accountRepository;
            this.accountMapper = accountMapper;
        }
        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId)
        {
            var allAccounts = accountRepository.GetAllAsync(1, int.MaxValue).GetAwaiter().GetResult();
            return allAccounts
                .Where(accounts => accounts.Id != excludeAccountId)
                .Select(accounts => accountMapper.ToDTO(accounts))
                .ToImmutableList();
        }
    }
}
