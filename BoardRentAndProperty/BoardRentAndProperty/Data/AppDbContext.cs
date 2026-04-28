namespace BoardRentAndProperty.Data
{
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
                        City NVARCHAR(100) NULL
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
                END;

                IF NOT EXISTS (SELECT * FROM [dbo].[Role] WHERE Name = 'Administrator')
                    INSERT INTO [dbo].[Role] (Id, Name) VALUES (NEWID(), 'Administrator');

                IF NOT EXISTS (SELECT * FROM [dbo].[Role] WHERE Name = 'Standard User')
                    INSERT INTO [dbo].[Role] (Id, Name) VALUES (NEWID(), 'Standard User');

                IF NOT EXISTS (SELECT * FROM [dbo].[Account] WHERE Username = 'admin')
                BEGIN
                    DECLARE @adminId UNIQUEIDENTIFIER = NEWID();
                    DECLARE @adminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Role] WHERE Name = 'Administrator');

                    INSERT INTO [dbo].[Account] (Id, Username, DisplayName, Email, PasswordHash, IsSuspended, CreatedAt, UpdatedAt)
                    VALUES (@adminId, 'admin', 'Administrator', 'admin@boardrent.com', '0Or88pPVbOSyUxu9djhSTw==:+uoeZ/oHtxEVK8bHfS5Eh/5chC0LoKdNvZjAVQhu7aw=', 0, GETUTCDATE(), GETUTCDATE());

                    INSERT INTO [dbo].[AccountRoles] (AccountId, RoleId) VALUES (@adminId, @adminRoleId);
                END;";

            command.ExecuteNonQuery();
        }
    }
}
