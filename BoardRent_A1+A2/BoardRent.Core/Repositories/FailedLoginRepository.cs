namespace BoardRent.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.Domain;
    using Microsoft.Data.SqlClient;
    public class FailedLoginRepository : IFailedLoginRepository
    {
        private IUnitOfWork unitOfWork;
        private SqlConnection Connection => this.unitOfWork.Connection;
        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<FailedLoginAttempt?> GetByUserIdAsync(Guid userId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM FailedLoginAttempt WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return this.MapFailedLoginAttempt(reader);
                    }
                }
            }

            return null;
        }
        public async Task IncrementAsync(Guid userId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"
                    IF EXISTS (SELECT 1 FROM FailedLoginAttempt WHERE UserId = @UserId)
                        UPDATE FailedLoginAttempt 
                        SET FailedAttempts = FailedAttempts + 1, 
                            LockedUntil = CASE WHEN FailedAttempts + 1 >= 5 THEN DATEADD(minute, 15, GETUTCDATE()) ELSE NULL END 
                        WHERE UserId = @UserId
                    ELSE
                        INSERT INTO FailedLoginAttempt (UserId, FailedAttempts, LockedUntil) VALUES (@UserId, 1, NULL)";

                command.Parameters.AddWithValue("@UserId", userId);

                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task ResetAsync(Guid userId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE FailedLoginAttempt
                    SET FailedAttempts = 0, LockedUntil = NULL
                    WHERE UserId = @UserId";

                command.Parameters.AddWithValue("@UserId", userId);

                await command.ExecuteNonQueryAsync();
            }
        }
        private FailedLoginAttempt MapFailedLoginAttempt(SqlDataReader reader)
        {
            return new FailedLoginAttempt
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
                LockedUntil = reader.IsDBNull(reader.GetOrdinal("LockedUntil")) ? null : reader.GetDateTime(reader.GetOrdinal("LockedUntil"))
            };
        }
    }
}