using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BoardRentAndProperty.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Account_OwnerId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Account_RecipientId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Requests_RelatedRequestId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications");

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"));

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Rentals",
                newName: "rental_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Notifications",
                newName: "notification_id");

            migrationBuilder.RenameColumn(
                name: "RelatedRequestId",
                table: "Notifications",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RelatedRequestId",
                table: "Notifications",
                newName: "IX_Notifications_user_id");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Games",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Games",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Games",
                newName: "image");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Games",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Games",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "MinimumPlayerNumber",
                table: "Games",
                newName: "minimum_player_number");

            migrationBuilder.RenameColumn(
                name: "MaximumPlayerNumber",
                table: "Games",
                newName: "maximum_player_number");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Games",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Games",
                newName: "game_id");

            migrationBuilder.RenameIndex(
                name: "IX_Games_OwnerId",
                table: "Games",
                newName: "IX_Games_owner_id");

            migrationBuilder.AlterColumn<int>(
                name: "RenterId",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingUserId",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RenterId",
                table: "Rentals",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Rentals",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "related_request_id",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "image",
                table: "Games",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "is_active",
                table: "Games",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "StreetNumber",
                table: "Account",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "StreetName",
                table: "Account",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Account",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Account",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Account",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Account",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "PamUserId",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Account_PamUserId",
                table: "Account",
                column: "PamUserId");

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "AvatarUrl", "City", "Country", "CreatedAt", "DisplayName", "Email", "IsSuspended", "PamUserId", "PasswordHash", "PhoneNumber", "StreetName", "StreetNumber", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator", "admin@boardrent.com", false, 4, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Darius Turcu", "darius@boardrent.com", false, 1, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "darius" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mihai Tira", "mihai@boardrent.com", false, 2, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mihai" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_related_request_id",
                table: "Notifications",
                column: "related_request_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Account_owner_id",
                table: "Games",
                column: "owner_id",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Account_user_id",
                table: "Notifications",
                column: "user_id",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Requests_related_request_id",
                table: "Notifications",
                column: "related_request_id",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests",
                column: "OfferingUserId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Account_owner_id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Account_user_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Requests_related_request_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_related_request_id",
                table: "Notifications");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Account_PamUserId",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "related_request_id",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PamUserId",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "rental_id",
                table: "Rentals",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "notification_id",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Notifications",
                newName: "RelatedRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                newName: "IX_Notifications_RelatedRequestId");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "Games",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Games",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "image",
                table: "Games",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Games",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Games",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "minimum_player_number",
                table: "Games",
                newName: "MinimumPlayerNumber");

            migrationBuilder.RenameColumn(
                name: "maximum_player_number",
                table: "Games",
                newName: "MaximumPlayerNumber");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Games",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "Games",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Games_owner_id",
                table: "Games",
                newName: "IX_Games_OwnerId");

            migrationBuilder.AlterColumn<Guid>(
                name: "RenterId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingUserId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RenterId",
                table: "Rentals",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Rentals",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Games",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Games",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Games",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "StreetNumber",
                table: "Account",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StreetName",
                table: "Account",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Account",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Account",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Account",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Account",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Account_OwnerId",
                table: "Games",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Account_RecipientId",
                table: "Notifications",
                column: "RecipientId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Requests_RelatedRequestId",
                table: "Notifications",
                column: "RelatedRequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests",
                column: "OfferingUserId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
