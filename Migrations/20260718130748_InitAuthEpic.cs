using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnJP.Migrations
{
    /// <inheritdoc />
    public partial class InitAuthEpic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "otp_verifications",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        OtpCodeHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Purpose = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AttemptCount = table.Column<int>(type: "int", nullable: false),
            //        IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_otp_verifications", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "users",
            //    columns: table => new
            //    {
            //        UserId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
            //        LockoutEndAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_users", x => x.UserId);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "password_reset_tokens",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        UserId = table.Column<int>(type: "int", nullable: false),
            //        TokenHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_password_reset_tokens_users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "users",
            //            principalColumn: "UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "user_learn",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        user_id = table.Column<int>(type: "int", nullable: false),
            //        level = table.Column<int>(type: "int", nullable: false),
            //        point = table.Column<int>(type: "int", nullable: false),
            //        time = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_user_learn", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_user_learn_users_user_id",
            //            column: x => x.user_id,
            //            principalTable: "users",
            //            principalColumn: "UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "userdetails",
            //    columns: table => new
            //    {
            //        UserId = table.Column<int>(type: "int", nullable: false),
            //        FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Gender = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
            //        Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        University = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Major = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Bio = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AvatarPublicId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_userdetails", x => x.UserId);
            //        table.ForeignKey(
            //            name: "FK_userdetails_users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "users",
            //            principalColumn: "UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_otp_verifications_Email_Purpose",
                table: "otp_verifications",
                columns: new[] { "Email", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_UserId",
                table: "password_reset_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_learn_user_id",
                table: "user_learn",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "otp_verifications");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "user_learn");

            migrationBuilder.DropTable(
                name: "userdetails");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
