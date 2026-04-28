namespace BoardRentAndProperty.Data
{
    using BoardRentAndProperty.Utilities;
    using Microsoft.Data.SqlClient;

    public class AppDbContext
    {
        private const string DatabaseName = "BoardRentDb";
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=BoardRentDb;Trusted_Connection=True;";
        private const string MasterConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;";
        private const string InitializationLockName = "BoardRentDb.Initialization";
        private const int InitializationLockTimeoutMilliseconds = 15000;
        private const string SeedDevPassword = "password123";

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public void EnsureCreated()
        {
            using (var masterConnection = new SqlConnection(MasterConnectionString))
            {
                masterConnection.Open();

                AcquireInitializationLock(masterConnection);

                try
                {
                    EnsureDatabaseExists(masterConnection);
                    EnsureSchemaCreated();
                }
                finally
                {
                    ReleaseInitializationLock(masterConnection);
                }
            }
        }

        private static void AcquireInitializationLock(SqlConnection masterConnection)
        {
            using var command = masterConnection.CreateCommand();
            command.CommandText = @"
                DECLARE @lockResult INT;
                EXEC @lockResult = sp_getapplock
                    @Resource = @Resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = @Timeout;

                IF @lockResult < 0
                BEGIN
                    THROW 51000, 'Unable to acquire the BoardRentDb initialization lock.', 1;
                END";
            command.Parameters.AddWithValue("@Resource", InitializationLockName);
            command.Parameters.AddWithValue("@Timeout", InitializationLockTimeoutMilliseconds);
            command.ExecuteNonQuery();
        }

        private static void ReleaseInitializationLock(SqlConnection masterConnection)
        {
            using var command = masterConnection.CreateCommand();
            command.CommandText = @"
                EXEC sp_releaseapplock
                    @Resource = @Resource,
                    @LockOwner = 'Session';";
            command.Parameters.AddWithValue("@Resource", InitializationLockName);
            command.ExecuteNonQuery();
        }

        private static void EnsureDatabaseExists(SqlConnection masterConnection)
        {
            using var command = masterConnection.CreateCommand();
            command.CommandText = $@"
                IF DB_ID(N'{DatabaseName}') IS NULL
                BEGIN
                    CREATE DATABASE [{DatabaseName}];
                END";
            command.ExecuteNonQuery();
        }

        private void EnsureSchemaCreated()
        {
            using var connection = this.CreateConnection();
            connection.Open();

            this.EnsureCoreTablesCreated(connection);
            this.EnsurePamUserIdColumnExists(connection);
            this.SeedRolesIfMissing(connection);
            this.SeedAccountIfMissing(
                connection,
                username: "admin",
                displayName: "Administrator",
                email: "admin@boardrent.com",
                roleName: "Administrator",
                pamUserId: null);
            this.SeedAccountIfMissing(
                connection,
                username: "darius",
                displayName: "Darius Turcu",
                email: "darius@boardrent.com",
                roleName: "Standard User",
                pamUserId: 1);
            this.SeedAccountIfMissing(
                connection,
                username: "mihai",
                displayName: "Mihai Tira",
                email: "mihai@boardrent.com",
                roleName: "Standard User",
                pamUserId: 2);
        }

        private void EnsureCoreTablesCreated(SqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF OBJECT_ID(N'[dbo].[Role]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Role] (
                        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                        Name NVARCHAR(50) NOT NULL UNIQUE
                    );
                END;

                IF OBJECT_ID(N'[dbo].[Account]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Account] (
                        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                        Username NVARCHAR(100) NOT NULL UNIQUE,
                        DisplayName NVARCHAR(200) NOT NULL,
                        Email NVARCHAR(200) NOT NULL UNIQUE,
                        PasswordHash NVARCHAR(500) NOT NULL,
                        PhoneNumber NVARCHAR(50) NULL,
                        AvatarUrl NVARCHAR(500) NULL,
                        IsSuspended BIT NOT NULL DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        StreetName NVARCHAR(200) NULL,
                        StreetNumber NVARCHAR(20) NULL,
                        Country NVARCHAR(100) NULL,
                        City NVARCHAR(100) NULL,
                        PamUserId INT NULL UNIQUE
                    );
                END;

                IF OBJECT_ID(N'[dbo].[AccountRoles]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[AccountRoles] (
                        AccountId UNIQUEIDENTIFIER NOT NULL,
                        RoleId UNIQUEIDENTIFIER NOT NULL,
                        PRIMARY KEY (AccountId, RoleId),
                        FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](Id) ON DELETE CASCADE,
                        FOREIGN KEY (RoleId) REFERENCES [dbo].[Role](Id) ON DELETE CASCADE
                    );
                END;

                IF OBJECT_ID(N'[dbo].[FailedLoginAttempt]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[FailedLoginAttempt] (
                        AccountId UNIQUEIDENTIFIER PRIMARY KEY,
                        FailedAttempts INT NOT NULL DEFAULT 0,
                        LockedUntil DATETIME2 NULL,
                        FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](Id) ON DELETE CASCADE
                    );
                END;";
            command.ExecuteNonQuery();
        }

        private void EnsurePamUserIdColumnExists(SqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[Account]')
                      AND name = N'PamUserId')
                BEGIN
                    ALTER TABLE [dbo].[Account] ADD PamUserId INT NULL;
                    ALTER TABLE [dbo].[Account] ADD CONSTRAINT UQ_Account_PamUserId UNIQUE (PamUserId);
                END;";
            command.ExecuteNonQuery();
        }

        private void SeedRolesIfMissing(SqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM [dbo].[Role] WHERE Name = 'Administrator')
                    INSERT INTO [dbo].[Role] (Id, Name) VALUES (NEWID(), 'Administrator');

                IF NOT EXISTS (SELECT 1 FROM [dbo].[Role] WHERE Name = 'Standard User')
                    INSERT INTO [dbo].[Role] (Id, Name) VALUES (NEWID(), 'Standard User');";
            command.ExecuteNonQuery();
        }

        private void SeedAccountIfMissing(
            SqlConnection connection,
            string username,
            string displayName,
            string email,
            string roleName,
            int? pamUserId)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM [dbo].[Account] WHERE Username = @Username)
                BEGIN
                    DECLARE @newAccountId UNIQUEIDENTIFIER = NEWID();
                    DECLARE @roleId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Role] WHERE Name = @RoleName);

                    INSERT INTO [dbo].[Account]
                        (Id, Username, DisplayName, Email, PasswordHash, IsSuspended, CreatedAt, UpdatedAt, PamUserId)
                    VALUES
                        (@newAccountId, @Username, @DisplayName, @Email, @PasswordHash, 0, GETUTCDATE(), GETUTCDATE(), @PamUserId);

                    IF @roleId IS NOT NULL
                        INSERT INTO [dbo].[AccountRoles] (AccountId, RoleId) VALUES (@newAccountId, @roleId);
                END;";
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@DisplayName", displayName);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@PasswordHash", PasswordHasher.HashPassword(SeedDevPassword));
            command.Parameters.AddWithValue("@RoleName", roleName);
            command.Parameters.AddWithValue("@PamUserId", (object?)pamUserId ?? System.DBNull.Value);
            command.ExecuteNonQuery();
        }
    }
}
