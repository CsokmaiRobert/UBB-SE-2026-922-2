using System;
using BoardRentAndProperty.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BoardRentAndProperty.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Account", entityBuilder =>
                {
                    entityBuilder.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    entityBuilder.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    entityBuilder.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    entityBuilder.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    entityBuilder.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    entityBuilder.Property<bool>("IsSuspended")
                        .HasColumnType("bit");

                    entityBuilder.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    entityBuilder.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    entityBuilder.Property<string>("StreetName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    entityBuilder.Property<string>("StreetNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    entityBuilder.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("Email")
                        .IsUnique();

                    entityBuilder.HasIndex("Username")
                        .IsUnique();

                    entityBuilder.ToTable("Account", (string)null);

                    entityBuilder.HasData(
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000010"),
                            AvatarUrl = "",
                            City = "",
                            Country = "",
                            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            DisplayName = "Administrator",
                            Email = "admin@boardrent.com",
                            IsSuspended = false,
                            PasswordHash = "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=",
                            PhoneNumber = "",
                            StreetName = "",
                            StreetNumber = "",
                            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Username = "admin"
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000011"),
                            AvatarUrl = "",
                            City = "",
                            Country = "",
                            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            DisplayName = "Darius Turcu",
                            Email = "darius@boardrent.com",
                            IsSuspended = false,
                            PasswordHash = "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=",
                            PhoneNumber = "",
                            StreetName = "",
                            StreetNumber = "",
                            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Username = "darius"
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000012"),
                            AvatarUrl = "",
                            City = "",
                            Country = "",
                            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            DisplayName = "Mihai Tira",
                            Email = "mihai@boardrent.com",
                            IsSuspended = false,
                            PasswordHash = "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=",
                            PhoneNumber = "",
                            StreetName = "",
                            StreetNumber = "",
                            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Username = "mihai"
                        });
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.AccountRole", entityBuilder =>
                {
                    entityBuilder.Property<Guid>("AccountId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.HasKey("AccountId", "RoleId");

                    entityBuilder.HasIndex("RoleId");

                    entityBuilder.ToTable("AccountRoles", (string)null);

                    entityBuilder.HasData(
                        new
                        {
                            AccountId = new Guid("00000000-0000-0000-0000-000000000010"),
                            RoleId = new Guid("00000000-0000-0000-0000-000000000001")
                        },
                        new
                        {
                            AccountId = new Guid("00000000-0000-0000-0000-000000000011"),
                            RoleId = new Guid("00000000-0000-0000-0000-000000000002")
                        },
                        new
                        {
                            AccountId = new Guid("00000000-0000-0000-0000-000000000012"),
                            RoleId = new Guid("00000000-0000-0000-0000-000000000002")
                        });
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.FailedLoginAttempt", entityBuilder =>
                {
                    entityBuilder.Property<Guid>("AccountId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<int>("FailedAttempts")
                        .HasColumnType("int");

                    entityBuilder.Property<DateTime?>("LockedUntil")
                        .HasColumnType("datetime2");

                    entityBuilder.HasKey("AccountId");

                    entityBuilder.ToTable("FailedLoginAttempt", (string)null);
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Game", entityBuilder =>
                {
                    entityBuilder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(entityBuilder.Property<int>("Id"));

                    entityBuilder.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    entityBuilder.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    entityBuilder.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    entityBuilder.Property<int>("MaximumPlayerNumber")
                        .HasColumnType("int");

                    entityBuilder.Property<int>("MinimumPlayerNumber")
                        .HasColumnType("int");

                    entityBuilder.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    entityBuilder.Property<Guid?>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("OwnerId");

                    entityBuilder.ToTable("Games", (string)null);
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Notification", entityBuilder =>
                {
                    entityBuilder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(entityBuilder.Property<int>("Id"));

                    entityBuilder.Property<string>("Body")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    entityBuilder.Property<Guid?>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<int?>("RelatedRequestId")
                        .HasColumnType("int");

                    entityBuilder.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    entityBuilder.Property<int>("Type")
                        .HasColumnType("int");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("RecipientId");

                    entityBuilder.HasIndex("RelatedRequestId");

                    entityBuilder.ToTable("Notifications", (string)null);
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Rental", entityBuilder =>
                {
                    entityBuilder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(entityBuilder.Property<int>("Id"));

                    entityBuilder.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<int?>("GameId")
                        .HasColumnType("int");

                    entityBuilder.Property<Guid?>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<Guid?>("RenterId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("GameId");

                    entityBuilder.HasIndex("OwnerId");

                    entityBuilder.HasIndex("RenterId");

                    entityBuilder.ToTable("Rentals", (string)null);
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Request", entityBuilder =>
                {
                    entityBuilder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(entityBuilder.Property<int>("Id"));

                    entityBuilder.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<int?>("GameId")
                        .HasColumnType("int");

                    entityBuilder.Property<Guid?>("OfferingUserId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<Guid?>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<Guid?>("RenterId")
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    entityBuilder.Property<int>("Status")
                        .HasColumnType("int");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("GameId");

                    entityBuilder.HasIndex("OfferingUserId");

                    entityBuilder.HasIndex("OwnerId");

                    entityBuilder.HasIndex("RenterId");

                    entityBuilder.ToTable("Requests", (string)null);
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Role", entityBuilder =>
                {
                    entityBuilder.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    entityBuilder.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    entityBuilder.HasKey("Id");

                    entityBuilder.HasIndex("Name")
                        .IsUnique();

                    entityBuilder.ToTable("Role", (string)null);

                    entityBuilder.HasData(
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            Name = "Administrator"
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000002"),
                            Name = "Standard User"
                        });
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.AccountRole", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    entityBuilder.Navigation("Account");

                    entityBuilder.Navigation("Role");
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.FailedLoginAttempt", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Account")
                        .WithOne()
                        .HasForeignKey("BoardRentAndProperty.Api.Models.FailedLoginAttempt", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    entityBuilder.Navigation("Account");
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Game", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.Navigation("Owner");
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Notification", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Request", "RelatedRequest")
                        .WithMany()
                        .HasForeignKey("RelatedRequestId")
                        .OnDelete(DeleteBehavior.SetNull);

                    entityBuilder.Navigation("Recipient");

                    entityBuilder.Navigation("RelatedRequest");
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Rental", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Renter")
                        .WithMany()
                        .HasForeignKey("RenterId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.Navigation("Game");

                    entityBuilder.Navigation("Owner");

                    entityBuilder.Navigation("Renter");
                });

            modelBuilder.Entity("BoardRentAndProperty.Api.Models.Request", entityBuilder =>
                {
                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "OfferingUser")
                        .WithMany()
                        .HasForeignKey("OfferingUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.HasOne("BoardRentAndProperty.Api.Models.Account", "Renter")
                        .WithMany()
                        .HasForeignKey("RenterId")
                        .OnDelete(DeleteBehavior.Restrict);

                    entityBuilder.Navigation("Game");

                    entityBuilder.Navigation("OfferingUser");

                    entityBuilder.Navigation("Owner");

                    entityBuilder.Navigation("Renter");
                });
#pragma warning restore 612, 618
        }
    }
}
