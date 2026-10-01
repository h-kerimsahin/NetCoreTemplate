using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace $safeprojectname$.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260930135542_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorType = table.Column<int>(type: "int", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                LockoutEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SystemLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LogLevel = table.Column<int>(type: "int", nullable: false),
                Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                ExceptionType = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                StackTrace = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                RequestPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                RequestMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SystemLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppUserActivities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ActivityType = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUserActivities", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppUserActivities_AppUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "AppUserProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUserProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppUserProfiles_AppUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppUserRefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                TokenType = table.Column<int>(type: "int", nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUserRefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppUserRefreshTokens_AppUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Settings", x => x.Id);
                table.ForeignKey(
                    name: "FK_Settings_AppUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppUserActivities_ActivityType",
            table: "AppUserActivities",
            column: "ActivityType");

        migrationBuilder.CreateIndex(
            name: "IX_AppUserActivities_AppUserId",
            table: "AppUserActivities",
            column: "AppUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AppUserActivities_EntityName_EntityId",
            table: "AppUserActivities",
            columns: new[] { "EntityName", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_AppUserProfiles_AppUserId",
            table: "AppUserProfiles",
            column: "AppUserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppUserRefreshTokens_AppUserId",
            table: "AppUserRefreshTokens",
            column: "AppUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AppUserRefreshTokens_Token",
            table: "AppUserRefreshTokens",
            column: "Token");

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Email",
            table: "AppUsers",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_UserName",
            table: "AppUsers",
            column: "UserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Settings_AppUserId",
            table: "Settings",
            column: "AppUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Settings_Key_AppUserId",
            table: "Settings",
            columns: new[] { "Key", "AppUserId" },
            unique: true,
            filter: "[AppUserId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_SystemLogs_CreatedDate",
            table: "SystemLogs",
            column: "CreatedDate");

        migrationBuilder.CreateIndex(
            name: "IX_SystemLogs_LogLevel",
            table: "SystemLogs",
            column: "LogLevel");

        migrationBuilder.CreateIndex(
            name: "IX_SystemLogs_UserId",
            table: "SystemLogs",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppUserActivities");

        migrationBuilder.DropTable(
            name: "AppUserProfiles");

        migrationBuilder.DropTable(
            name: "AppUserRefreshTokens");

        migrationBuilder.DropTable(
            name: "Settings");

        migrationBuilder.DropTable(
            name: "SystemLogs");

        migrationBuilder.DropTable(
            name: "AppUsers");
    }
}
