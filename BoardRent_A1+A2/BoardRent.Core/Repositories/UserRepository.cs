namespace BoardRent.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.Domain;
    using Microsoft.Data.SqlClient;

    public class UserRepository : IUserRepository
    {
        private IUnitOfWork unitOfWork;

        private SqlConnection DatabaseConnection => this.unitOfWork.Connection;

        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<User> GetByIdAsync(Guid identifier)
        {
            User userEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [User] WHERE Id = @Identifier";
                sqlCommand.Parameters.AddWithValue("@Identifier", identifier);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        userEntity = this.MapDataReaderToUserEntity(dataReader);
                    }
                }
            }

            if (userEntity != null)
            {
                userEntity.Roles = await this.LoadUserRolesAsync(userEntity.Id);
            }

            return userEntity;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            User userEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [User] WHERE Username = @Username";
                sqlCommand.Parameters.AddWithValue("@Username", username);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        userEntity = this.MapDataReaderToUserEntity(dataReader);
                    }
                }
            }

            if (userEntity != null)
            {
                userEntity.Roles = await this.LoadUserRolesAsync(userEntity.Id);
            }

            return userEntity;
        }

        public async Task<User> GetByEmailAsync(string emailAddress)
        {
            User userEntity = null;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [User] WHERE Email = @EmailAddress";
                sqlCommand.Parameters.AddWithValue("@EmailAddress", emailAddress);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        userEntity = this.MapDataReaderToUserEntity(dataReader);
                    }
                }
            }

            if (userEntity != null)
            {
                userEntity.Roles = await this.LoadUserRolesAsync(userEntity.Id);
            }

            return userEntity;
        }

        public async Task<List<User>> GetAllAsync(int pageNumber, int pageSize)
        {
            var userList = new List<User>();
            const int PaginationOffsetAdjustment = 1;
            int offsetCalculation = (pageNumber - PaginationOffsetAdjustment) * pageSize;

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT * FROM [User] ORDER BY CreatedAt OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                sqlCommand.Parameters.AddWithValue("@Offset", offsetCalculation);
                sqlCommand.Parameters.AddWithValue("@PageSize", pageSize);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await dataReader.ReadAsync())
                    {
                        userList.Add(this.MapDataReaderToUserEntity(dataReader));
                    }
                }
            }

            foreach (var userEntity in userList)
            {
                userEntity.Roles = await this.LoadUserRolesAsync(userEntity.Id);
            }

            return userList;
        }

        public async Task AddAsync(User userEntity)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    INSERT INTO [User] (Id, Username, DisplayName, Email, PasswordHash, PhoneNumber, AvatarUrl, IsSuspended, CreatedAt, UpdatedAt, StreetName, StreetNumber, Country, City)
                    VALUES (@Identifier, @Username, @DisplayName, @EmailAddress, @PasswordHash, @PhoneNumber, @AvatarUrl, @IsSuspended, @CreatedAt, @UpdatedAt, @StreetName, @StreetNumber, @Country, @City)";

                sqlCommand.Parameters.AddWithValue("@Identifier", userEntity.Id);
                sqlCommand.Parameters.AddWithValue("@Username", userEntity.Username);
                sqlCommand.Parameters.AddWithValue("@DisplayName", userEntity.DisplayName);
                sqlCommand.Parameters.AddWithValue("@EmailAddress", userEntity.Email);
                sqlCommand.Parameters.AddWithValue("@PasswordHash", userEntity.PasswordHash);
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", userEntity.PhoneNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@AvatarUrl", userEntity.AvatarUrl ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@IsSuspended", userEntity.IsSuspended);
                sqlCommand.Parameters.AddWithValue("@CreatedAt", userEntity.CreatedAt);
                sqlCommand.Parameters.AddWithValue("@UpdatedAt", userEntity.UpdatedAt);
                sqlCommand.Parameters.AddWithValue("@StreetName", userEntity.StreetName ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StreetNumber", userEntity.StreetNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@Country", userEntity.Country ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@City", userEntity.City ?? (object)DBNull.Value);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(User userEntity)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    UPDATE [User] SET
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
                        City = @City
                    WHERE Id = @Identifier";

                sqlCommand.Parameters.AddWithValue("@Identifier", userEntity.Id);
                sqlCommand.Parameters.AddWithValue("@DisplayName", userEntity.DisplayName);
                sqlCommand.Parameters.AddWithValue("@EmailAddress", userEntity.Email);
                sqlCommand.Parameters.AddWithValue("@PasswordHash", userEntity.PasswordHash);
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", userEntity.PhoneNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@AvatarUrl", userEntity.AvatarUrl ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@IsSuspended", userEntity.IsSuspended);
                sqlCommand.Parameters.AddWithValue("@UpdatedAt", userEntity.UpdatedAt);
                sqlCommand.Parameters.AddWithValue("@StreetName", userEntity.StreetName ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StreetNumber", userEntity.StreetNumber ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@Country", userEntity.Country ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@City", userEntity.City ?? (object)DBNull.Value);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task AddRoleAsync(Guid identifier, string roleName)
        {
            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    DECLARE @RoleIdentifier UNIQUEIDENTIFIER = (SELECT Id FROM Role WHERE Name = @RoleName);
                    IF @RoleIdentifier IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserIdentifier AND RoleId = @RoleIdentifier)
                        INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserIdentifier, @RoleIdentifier);";

                sqlCommand.Parameters.AddWithValue("@UserIdentifier", identifier);
                sqlCommand.Parameters.AddWithValue("@RoleName", roleName);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<Role>> LoadUserRolesAsync(Guid identifier)
        {
            var roleList = new List<Role>();

            using (var sqlCommand = this.DatabaseConnection.CreateCommand())
            {
                sqlCommand.CommandText = @"
                    SELECT TargetRole.Id, TargetRole.Name
                    FROM Role TargetRole
                    INNER JOIN UserRoles PivotTable ON PivotTable.RoleId = TargetRole.Id
                    WHERE PivotTable.UserId = @UserIdentifier";

                sqlCommand.Parameters.AddWithValue("@UserIdentifier", identifier);

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

        private User MapDataReaderToUserEntity(SqlDataReader dataReader)
        {
            return new User
            {
                Id = dataReader.GetGuid(dataReader.GetOrdinal("Id")),
                Username = dataReader.GetString(dataReader.GetOrdinal("Username")),
                DisplayName = dataReader.GetString(dataReader.GetOrdinal("DisplayName")),
                Email = dataReader.GetString(dataReader.GetOrdinal("Email")),
                PasswordHash = dataReader.GetString(dataReader.GetOrdinal("PasswordHash")),
                PhoneNumber = dataReader.IsDBNull(dataReader.GetOrdinal("PhoneNumber")) ? null : dataReader.GetString(dataReader.GetOrdinal("PhoneNumber")),
                AvatarUrl = dataReader.IsDBNull(dataReader.GetOrdinal("AvatarUrl")) ? null : dataReader.GetString(dataReader.GetOrdinal("AvatarUrl")),
                IsSuspended = dataReader.GetBoolean(dataReader.GetOrdinal("IsSuspended")),
                CreatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("CreatedAt")),
                UpdatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("UpdatedAt")),
                StreetName = dataReader.IsDBNull(dataReader.GetOrdinal("StreetName")) ? null : dataReader.GetString(dataReader.GetOrdinal("StreetName")),
                StreetNumber = dataReader.IsDBNull(dataReader.GetOrdinal("StreetNumber")) ? null : dataReader.GetString(dataReader.GetOrdinal("StreetNumber")),
                Country = dataReader.IsDBNull(dataReader.GetOrdinal("Country")) ? null : dataReader.GetString(dataReader.GetOrdinal("Country")),
                City = dataReader.IsDBNull(dataReader.GetOrdinal("City")) ? null : dataReader.GetString(dataReader.GetOrdinal("City")),
                Roles = new List<Role>()
            };
        }
    }
}