namespace BoardRentAndProperty.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.Mappers;
    using BoardRentAndProperty.Models;
    using Microsoft.Data.SqlClient;

    public class AccountRepository : IAccountRepository
    {
        private readonly AccountMapper accountMapper;
        private IUnitOfWork unitOfWork;

        public AccountRepository(AccountMapper accountMapper)
        {
            this.accountMapper = accountMapper;
        }

        private SqlConnection DatabaseConnection => this.unitOfWork.Connection;

        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Account> GetByIdAsync(Guid identifier)
        {
            Account accountEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [Account] WHERE Id = @Identifier";
                sqlCommand.Parameters.AddWithValue("@Identifier", identifier);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        accountEntity = this.accountMapper.FromReader(dataReader);
                    }
                }
            }

            if (accountEntity != null)
            {
                accountEntity.Roles = await this.LoadAccountRolesAsync(accountEntity.Id);
            }

            return accountEntity;
        }

        public async Task<Account> GetByUsernameAsync(string username)
        {
            Account accountEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [Account] WHERE Username = @Username";
                sqlCommand.Parameters.AddWithValue("@Username", username);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        accountEntity = this.accountMapper.FromReader(dataReader);
                    }
                }
            }

            if (accountEntity != null)
            {
                accountEntity.Roles = await this.LoadAccountRolesAsync(accountEntity.Id);
            }

            return accountEntity;
        }

        public async Task<Account> GetByEmailAsync(string emailAddress)
        {
            Account accountEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [Account] WHERE Email = @EmailAddress";
                sqlCommand.Parameters.AddWithValue("@EmailAddress", emailAddress);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        accountEntity = this.accountMapper.FromReader(dataReader);
                    }
                }
            }

            if (accountEntity != null)
            {
                accountEntity.Roles = await this.LoadAccountRolesAsync(accountEntity.Id);
            }

            return accountEntity;
        }

        public async Task<List<Account>> GetAllAsync(int pageNumber, int pageSize)
        {
            var accountList = new List<Account>();
            const int PaginationOffsetAdjustment = 1;
            int offsetCalculation = (pageNumber - PaginationOffsetAdjustment) * pageSize;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [Account] ORDER BY CreatedAt OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                sqlCommand.Parameters.AddWithValue("@Offset", offsetCalculation);
                sqlCommand.Parameters.AddWithValue("@PageSize", pageSize);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await dataReader.ReadAsync())
                    {
                        accountList.Add(this.accountMapper.FromReader(dataReader));
                    }
                }
            }

            foreach (var accountEntity in accountList)
            {
                accountEntity.Roles = await this.LoadAccountRolesAsync(accountEntity.Id);
            }

            return accountList;
        }

        public async Task AddAsync(Account accountEntity)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    INSERT INTO [Account] (Id, Username, DisplayName, Email, PasswordHash, PhoneNumber, AvatarUrl, IsSuspended, CreatedAt, UpdatedAt, StreetName, StreetNumber, Country, City, PamUserId)
                    VALUES (@Identifier, @Username, @DisplayName, @EmailAddress, @PasswordHash, @PhoneNumber, @AvatarUrl, @IsSuspended, @CreatedAt, @UpdatedAt, @StreetName, @StreetNumber, @Country, @City, @PamUserId)";

                sqlCommand.Parameters.AddWithValue("@Identifier", accountEntity.Id);
                sqlCommand.Parameters.AddWithValue("@Username", accountEntity.Username);
                sqlCommand.Parameters.AddWithValue("@DisplayName", accountEntity.DisplayName);
                sqlCommand.Parameters.AddWithValue("@EmailAddress", accountEntity.Email);
                sqlCommand.Parameters.AddWithValue("@PasswordHash", accountEntity.PasswordHash);
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", accountEntity.PhoneNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@AvatarUrl", accountEntity.AvatarUrl ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@IsSuspended", accountEntity.IsSuspended);
                sqlCommand.Parameters.AddWithValue("@CreatedAt", accountEntity.CreatedAt);
                sqlCommand.Parameters.AddWithValue("@UpdatedAt", accountEntity.UpdatedAt);
                sqlCommand.Parameters.AddWithValue("@StreetName", accountEntity.StreetName ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StreetNumber", accountEntity.StreetNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@Country", accountEntity.Country ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@City", accountEntity.City ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@PamUserId", (object?)accountEntity.PamUserId ?? DBNull.Value);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(Account accountEntity)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    UPDATE [Account] SET
                        DisplayName = @DisplayName,
                        Email = @EmailAddress,
                        PasswordHash = @PasswordHash,
                        PhoneNumber = @PhoneNumber,
                        AvatarUrl = @AvatarUrl,
                        IsSuspended = @IsSuspended,
                        UpdatedAt = @UpdatedAt,
                        StreetName = @StreetName,
                        StreetNumber = @StreetNumber,
                        Country = @Country,
                        City = @City,
                        PamUserId = @PamUserId
                    WHERE Id = @Identifier";

                sqlCommand.Parameters.AddWithValue("@Identifier", accountEntity.Id);
                sqlCommand.Parameters.AddWithValue("@DisplayName", accountEntity.DisplayName);
                sqlCommand.Parameters.AddWithValue("@EmailAddress", accountEntity.Email);
                sqlCommand.Parameters.AddWithValue("@PasswordHash", accountEntity.PasswordHash);
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", accountEntity.PhoneNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@AvatarUrl", accountEntity.AvatarUrl ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@IsSuspended", accountEntity.IsSuspended);
                sqlCommand.Parameters.AddWithValue("@UpdatedAt", accountEntity.UpdatedAt);
                sqlCommand.Parameters.AddWithValue("@StreetName", accountEntity.StreetName ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StreetNumber", accountEntity.StreetNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@Country", accountEntity.Country ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@City", accountEntity.City ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@PamUserId", (object?)accountEntity.PamUserId ?? DBNull.Value);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task SetPamUserIdAsync(Guid accountId, int pamUserId)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "UPDATE [Account] SET PamUserId = @PamUserId WHERE Id = @AccountIdentifier";
                sqlCommand.Parameters.AddWithValue("@PamUserId", pamUserId);
                sqlCommand.Parameters.AddWithValue("@AccountIdentifier", accountId);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task AddRoleAsync(Guid identifier, string roleName)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    DECLARE @RoleIdentifier UNIQUEIDENTIFIER = (SELECT Id FROM Role WHERE Name = @RoleName);
                    IF @RoleIdentifier IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AccountRoles WHERE AccountId = @AccountIdentifier AND RoleId = @RoleIdentifier)
                        INSERT INTO AccountRoles (AccountId, RoleId) VALUES (@AccountIdentifier, @RoleIdentifier);";

                sqlCommand.Parameters.AddWithValue("@AccountIdentifier", identifier);
                sqlCommand.Parameters.AddWithValue("@RoleName", roleName);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<Role>> LoadAccountRolesAsync(Guid identifier)
        {
            var roleList = new List<Role>();

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    SELECT TargetRole.Id, TargetRole.Name
                    FROM Role TargetRole
                    INNER JOIN AccountRoles PivotTable ON PivotTable.RoleId = TargetRole.Id
                    WHERE PivotTable.AccountId = @AccountIdentifier";

                sqlCommand.Parameters.AddWithValue("@AccountIdentifier", identifier);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await dataReader.ReadAsync())
                    {
                        roleList.Add(new Role
                        {
                            Id = dataReader.GetGuid(dataReader.GetOrdinal("Id")),
                            Name = dataReader.GetString(dataReader.GetOrdinal("Name"))
                        });
                    }
                }
            }

            return roleList;
        }
    }
}
