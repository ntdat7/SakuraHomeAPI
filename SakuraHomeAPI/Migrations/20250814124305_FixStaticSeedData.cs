using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixStaticSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "Avatar", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "DateOfBirth", "DeletedAt", "DeletedBy", "Email", "EmailConfirmed", "EmailNotifications", "EmailVerificationToken", "EmailVerified", "EmailVerifiedAt", "ExternalId", "FailedLoginAttempts", "FirstName", "Gender", "IsActive", "IsDeleted", "LastLoginAt", "LastLoginIp", "LastName", "LastOrderDate", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "NotificationPreferences", "PasswordHash", "PasswordResetExpires", "PasswordResetToken", "PhoneNumber", "PhoneNumberConfirmed", "PhoneVerificationCode", "PhoneVerified", "PhoneVerifiedAt", "Points", "PreferredCurrency", "PreferredLanguage", "Provider", "PushNotifications", "Role", "SecurityStamp", "SmsNotifications", "Status", "Tier", "TotalOrders", "TotalSpent", "TwoFactorEnabled", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 0, null, "11111111-1111-1111-1111-111111111111", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, null, "superadmin@sakurahome.com", true, true, null, true, null, null, 0, "Super", null, true, false, null, null, "Admin", null, false, null, null, null, null, "AQAAAAIAAYagAAAAEEV7OG+DPOtR9KwqdzspiSFEm2Q00X7fYjWfn7fhRI0+8R/F1rFeEV1+CLyGtmKtxw==", null, null, null, false, null, false, null, 0, "VND", "vi", 1, true, 4, "b1e2c3d4-5678-1234-9876-abcdefabcdef", false, 2, 1, 0, 0m, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
