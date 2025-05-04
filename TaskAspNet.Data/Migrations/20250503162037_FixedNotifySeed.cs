using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAspNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedNotifySeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 1,
                column: "Roles",
                value: "User,Admin,SuperAdmin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 1,
                column: "Roles",
                value: "User");
        }
    }
}
