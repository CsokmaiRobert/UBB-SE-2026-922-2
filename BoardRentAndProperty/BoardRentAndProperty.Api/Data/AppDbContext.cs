using System;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Data
{
    public class AppDbContext : DbContext
    {
        private static readonly Guid AdminRoleId = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StandardUserRoleId = new Guid("00000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminAccountId = new Guid("00000000-0000-0000-0000-000000000010");
        private static readonly Guid DariusAccountId = new Guid("00000000-0000-0000-0000-000000000011");
        private static readonly Guid MihaiAccountId = new Guid("00000000-0000-0000-0000-000000000012");

        private const string SeedDevPassword = "password123";

        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; } = default!;
        public DbSet<Game> Games { get; set; } = default!;
        public DbSet<Rental> Rentals { get; set; } = default!;
        public DbSet<Request> Requests { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");
                entity.HasKey(account => account.Id);
                entity.Property(account => account.Username).HasMaxLength(100).IsRequired();
                entity.Property(account => account.DisplayName).HasMaxLength(200).IsRequired();
                entity.Property(account => account.Email).HasMaxLength(200).IsRequired();
                entity.Property(account => account.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(account => account.PhoneNumber).HasMaxLength(50);
                entity.Property(account => account.AvatarUrl).HasMaxLength(500);
                entity.Property(account => account.StreetName).HasMaxLength(200);
                entity.Property(account => account.StreetNumber).HasMaxLength(20);
                entity.Property(account => account.Country).HasMaxLength(100);
                entity.Property(account => account.City).HasMaxLength(100);
                entity.HasIndex(account => account.Username).IsUnique();
                entity.HasIndex(account => account.Email).IsUnique();
                entity.HasMany(account => account.Roles)
                      .WithMany()
                      .UsingEntity<AccountRole>(
                          joinEntity => joinEntity.HasOne(accountRole => accountRole.Role).WithMany().HasForeignKey(accountRole => accountRole.RoleId).OnDelete(DeleteBehavior.Cascade),
                          joinEntity => joinEntity.HasOne(accountRole => accountRole.Account).WithMany().HasForeignKey(accountRole => accountRole.AccountId).OnDelete(DeleteBehavior.Cascade),
                          joinEntity =>
                          {
                              joinEntity.ToTable("AccountRoles");
                              joinEntity.HasKey(accountRole => new { accountRole.AccountId, accountRole.RoleId });
                          });
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(role => role.Id);
                entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(role => role.Name).IsUnique();
            });

            modelBuilder.Entity<FailedLoginAttempt>(entity =>
            {
                entity.ToTable("FailedLoginAttempt");
                entity.HasKey(failedLogin => failedLogin.AccountId);
                entity.HasOne(failedLogin => failedLogin.Account).WithOne().HasForeignKey<FailedLoginAttempt>(failedLogin => failedLogin.AccountId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasKey(game => game.Id);
                entity.Property(game => game.Id).ValueGeneratedOnAdd();
                entity.Property(game => game.Name).HasMaxLength(100).IsRequired();
                entity.Property(game => game.Price).HasColumnType("decimal(10,2)");
                entity.Property(game => game.Image).HasColumnType("varbinary(max)");
                entity.HasOne(game => game.Owner).WithMany().HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rental>(entity =>
            {
                entity.ToTable("Rentals");
                entity.HasKey(rental => rental.Id);
                entity.Property(rental => rental.Id).ValueGeneratedOnAdd();
                entity.HasOne(rental => rental.Game).WithMany().HasForeignKey("GameId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(rental => rental.Renter).WithMany().HasForeignKey("RenterId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(rental => rental.Owner).WithMany().HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");
                entity.HasKey(request => request.Id);
                entity.Property(request => request.Id).ValueGeneratedOnAdd();
                entity.HasOne(request => request.Game).WithMany().HasForeignKey("GameId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.Renter).WithMany().HasForeignKey("RenterId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.Owner).WithMany().HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.OfferingUser).WithMany().HasForeignKey("OfferingUserId").OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(notification => notification.Id);
                entity.Property(notification => notification.Id).ValueGeneratedOnAdd();
                entity.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
                entity.Property(notification => notification.Body).HasMaxLength(2000).IsRequired();
                entity.HasOne(notification => notification.Recipient).WithMany().HasForeignKey("RecipientId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(notification => notification.RelatedRequest).WithMany().HasForeignKey("RelatedRequestId").OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            });

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = AdminRoleId,
                    Name = "Administrator",
                },
                new Role
                {
                    Id = StandardUserRoleId,
                    Name = "Standard User",
                });

            var seedHash = PasswordHasher.HashPassword(SeedDevPassword);
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = AdminAccountId,
                    Username = "admin",
                    DisplayName = "Administrator",
                    Email = "admin@boardrent.com",
                    PasswordHash = seedHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                },
                new Account
                {
                    Id = DariusAccountId,
                    Username = "darius",
                    DisplayName = "Darius Turcu",
                    Email = "darius@boardrent.com",
                    PasswordHash = seedHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                },
                new Account
                {
                    Id = MihaiAccountId,
                    Username = "mihai",
                    DisplayName = "Mihai Tira",
                    Email = "mihai@boardrent.com",
                    PasswordHash = seedHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                });

            modelBuilder.Entity<AccountRole>().HasData(
                new AccountRole
                {
                    AccountId = AdminAccountId,
                    RoleId = AdminRoleId,
                },
                new AccountRole
                {
                    AccountId = DariusAccountId,
                    RoleId = StandardUserRoleId,
                },
                new AccountRole
                {
                    AccountId = MihaiAccountId,
                    RoleId = StandardUserRoleId,
                });
        }
    }
}
