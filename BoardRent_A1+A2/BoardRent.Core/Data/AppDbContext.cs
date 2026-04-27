namespace BoardRent.Data
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    public class AppDbContext
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=BoardRentDb;Trusted_Connection=True;";

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public void EnsureCreated()
        {
            using (var masterConnection = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;"))
            {
                masterConnection.Open();
                using var masterCommand = masterConnection.CreateCommand();
                masterCommand.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BoardRentDb')
                    CREATE DATABASE BoardRentDb;
                ";
                masterCommand.ExecuteNonQuery();
            }

            using var connection = this.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Role')
                CREATE TABLE Role (
                    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    Name NVARCHAR(50) NOT NULL UNIQUE
                );
                
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User')
                CREATE TABLE [User] (
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

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
                CREATE TABLE UserRoles (
                    UserId UNIQUEIDENTIFIER NOT NULL,
                    RoleId UNIQUEIDENTIFIER NOT NULL,
                    PRIMARY KEY (UserId, RoleId),
                    FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE,
                    FOREIGN KEY (RoleId) REFERENCES Role(Id) ON DELETE CASCADE
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FailedLoginAttempt')
                CREATE TABLE FailedLoginAttempt (
                    UserId UNIQUEIDENTIFIER PRIMARY KEY,
                    FailedAttempts INT NOT NULL DEFAULT 0,
                    LockedUntil DATETIME2 NULL,
                    FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE
                );

                IF NOT EXISTS (SELECT * FROM Role WHERE Name = 'Administrator')
                    INSERT INTO Role (Id, Name) VALUES (NEWID(), 'Administrator');

                IF NOT EXISTS (SELECT * FROM Role WHERE Name = 'Standard User')
                    INSERT INTO Role (Id, Name) VALUES (NEWID(), 'Standard User');

                IF NOT EXISTS (SELECT * FROM [User] WHERE Username = 'admin')
                BEGIN
                    DECLARE @adminId UNIQUEIDENTIFIER = NEWID();
                    DECLARE @adminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Role WHERE Name = 'Administrator');

                    INSERT INTO [User] (Id, Username, DisplayName, Email, PasswordHash, IsSuspended, CreatedAt, UpdatedAt)
                    VALUES (@adminId, 'admin', 'Administrator', 'admin@boardrent.com', '0Or88pPVbOSyUxu9djhSTw==:+uoeZ/oHtxEVK8bHfS5Eh/5chC0LoKdNvZjAVQhu7aw=', 0, GETUTCDATE(), GETUTCDATE());

                    INSERT INTO UserRoles (UserId, RoleId) VALUES (@adminId, @adminRoleId);
                END
            ";

            command.ExecuteNonQuery();
        }
    }
}