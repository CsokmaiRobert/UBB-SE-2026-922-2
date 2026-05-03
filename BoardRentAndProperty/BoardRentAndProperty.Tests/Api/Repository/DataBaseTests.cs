using System;
using System.IO;
using System.Text.Json;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Repository
{
    public abstract class DataBaseTests
    {
        protected static readonly Guid OwnerAccountId = new Guid("00000000-0000-0000-0000-000000000011");
        protected static readonly Guid RenterAccountId = new Guid("00000000-0000-0000-0000-000000000012");

        protected string ConnectionString { get; private set; } = string.Empty;
        protected IDbContextFactory<AppDbContext> DbContextFactory { get; private set; } = null!;

        [OneTimeSetUp]
        public void InitializeDatabase()
        {
            this.ConnectionString = ResolveConnectionString();
            if (string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                Assert.Ignore("Connection string is missing.");
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(this.ConnectionString)
                .Options;

            this.DbContextFactory = new PooledDbContextFactory<AppDbContext>(options);

            try
            {
                using var dbContext = this.DbContextFactory.CreateDbContext();
                dbContext.Database.Migrate();
            }
            catch (SqlException)
            {
                Assert.Ignore("SQL Server is not reachable.");
            }
        }

        [SetUp]
        public void ResetBusinessTables()
        {
            try
            {
                using var dbContext = this.DbContextFactory.CreateDbContext();
                dbContext.Database.ExecuteSqlRaw(
                    "DELETE FROM Notifications;" +
                    "DELETE FROM Rentals;" +
                    "DELETE FROM Requests;" +
                    "DELETE FROM Games;" +
                    "DBCC CHECKIDENT ('Notifications', RESEED, 0);" +
                    "DBCC CHECKIDENT ('Rentals', RESEED, 0);" +
                    "DBCC CHECKIDENT ('Requests', RESEED, 0);" +
                    "DBCC CHECKIDENT ('Games', RESEED, 0);");
            }
            catch (SqlException sqlException)
            {
                Assert.Ignore($"Skipping integration tests because SQL Server is not reachable. Error: {sqlException.Message}");
            }
        }

        protected int SeedGame(Guid ownerAccountId, string gameName = "Seed Game")
        {
            using var dbContext = this.DbContextFactory.CreateDbContext();

            var game = new Game
            {
                Owner = dbContext.Accounts.Find(ownerAccountId),
                Name = gameName,
                Price = 10m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "A seeded game used by integration tests.",
                Image = Array.Empty<byte>(),
                IsActive = true,
            };

            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            return game.Id;
        }

        private static string ResolveConnectionString()
        {
            string? currentDirectory = TestContext.CurrentContext.TestDirectory;

            while (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                string candidatePath = Path.Combine(currentDirectory, "BoardRentAndProperty.Api", "appsettings.json");
                if (File.Exists(candidatePath))
                {
                    using var jsonDocument = JsonDocument.Parse(File.ReadAllText(candidatePath));
                    if (jsonDocument.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                        && connectionStrings.TryGetProperty("BoardRentAndProperty", out var connectionStringValue))
                    {
                        return connectionStringValue.GetString() ?? string.Empty;
                    }
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return string.Empty;
        }
    }
}
