using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskAspNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedForNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTargetGroups",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Roles" },
                values: new object[,]
                {
                    { 1, "All user-level notifications", true, "Users", "User" },
                    { 2, "All admin-level notifications", true, "Administrators", "Admin,SuperAdmin" },
                    { 3, "All client-level notifications", true, "Clients", "Client" }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "DefaultMessageTemplate", "Description", "IsActive", "Name", "TargetGroupId" },
                values: new object[,]
                {
                    { 1, "New member '{0}' has been created", "Fires when a new member is created", true, "MemberCreated", 2 },
                    { 2, "Member '{0}' has been updated", "Fires when a member is updated", true, "MemberUpdated", 2 },
                    { 3, "New project '{0}' has been created", "Fires when a new project is created", true, "ProjectCreated", 1 },
                    { 4, "Project '{0}' has been updated", "Fires when a project is updated", true, "ProjectUpdated", 1 },
                    { 5, "Member '{0}' added to project '{1}'", "Fires when a member is added to a project", true, "MemberAddedToProject", 2 },
                    { 6, "Member '{0}' removed from project '{1}'", "Fires when a member is removed from a project", true, "MemberRemovedFromProject", 2 },
                    { 7, "New client '{0}' has been created", "Fires when a new client is created", true, "ClientCreated", 3 },
                    { 8, "Client '{0}' has been updated", "Fires when a client is updated", true, "ClientUpdated", 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NotificationTargetGroups",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
