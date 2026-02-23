using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskGroupKeyToMaintenanceRequestTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLead",
                table: "MaintenanceRequestTechnician",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaskGroupKey",
                table: "MaintenanceRequestTechnician",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianStatus",
                table: "MaintenanceRequestTechnician",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianAssignmentMode",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLead",
                table: "MaintenanceRequestTechnician");

            migrationBuilder.DropColumn(
                name: "TaskGroupKey",
                table: "MaintenanceRequestTechnician");

            migrationBuilder.DropColumn(
                name: "TechnicianStatus",
                table: "MaintenanceRequestTechnician");

            migrationBuilder.DropColumn(
                name: "TechnicianAssignmentMode",
                table: "MaintenanceRequests");
        }
    }
}
