using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAspNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class MinorChangeToSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 3,
                column: "Roles",
                value: "User,Admin,SuperAdmin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 3,
                column: "Roles",
                value: "Client");
        }
    }
}
