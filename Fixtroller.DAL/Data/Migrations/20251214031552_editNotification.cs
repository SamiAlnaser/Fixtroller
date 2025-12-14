using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class editNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_MaintenanceRequests_MaintenanceRequestId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_MaintenanceRequestId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Notifications",
                newName: "TitleKey");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Notifications",
                newName: "BodyKey");

            migrationBuilder.AddColumn<string>(
                name: "BodyArgsJson",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleArgsJson",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyArgsJson",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TitleArgsJson",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TitleKey",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "BodyKey",
                table: "Notifications",
                newName: "Body");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MaintenanceRequestId",
                table: "Notifications",
                column: "MaintenanceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_MaintenanceRequests_MaintenanceRequestId",
                table: "Notifications",
                column: "MaintenanceRequestId",
                principalTable: "MaintenanceRequests",
                principalColumn: "Id");
        }
    }
}
