namespace BoardRentAndProperty.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRentAndProperty.Data;
    using BoardRentAndProperty.Models;
    using Microsoft.Data.SqlClient;
    public class FailedLoginRepository : IFailedLoginRepository
    {
        private IUnitOfWork unitOfWork;
        private SqlConnection Connection => this.unitOfWork.Connection;
        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<FailedLoginAttempt?> GetByAccountIdAsync(Guid accountId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM FailedLoginAttempt WHERE AccountId = @AccountId";
                command.Parameters.AddWithValue("@AccountId", accountId);

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
        public async Task IncrementAsync(Guid accountId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"
                    IF EXISTS (SELECT 1 FROM FailedLoginAttempt WHERE AccountId = @AccountId)
                        UPDATE FailedLoginAttempt
                        SET FailedAttempts = FailedAttempts + 1,
                            LockedUntil = CASE WHEN FailedAttempts + 1 >= 5 THEN DATEADD(minute, 15, GETUTCDATE()) ELSE NULL END
                        WHERE AccountId = @AccountId
                    ELSE
                        INSERT INTO FailedLoginAttempt (AccountId, FailedAttempts, LockedUntil) VALUES (@AccountId, 1, NULL)";

                command.Parameters.AddWithValue("@AccountId", accountId);

                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task ResetAsync(Guid accountId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE FailedLoginAttempt
                    SET FailedAttempts = 0, LockedUntil = NULL
                    WHERE AccountId = @AccountId";

                command.Parameters.AddWithValue("@AccountId", accountId);

                await command.ExecuteNonQueryAsync();
            }
        }
        private FailedLoginAttempt MapFailedLoginAttempt(SqlDataReader reader)
        {
            return new FailedLoginAttempt
            {
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
                LockedUntil = reader.IsDBNull(reader.GetOrdinal("LockedUntil")) ? null : reader.GetDateTime(reader.GetOrdinal("LockedUntil"))
            };
        }
    }
}
