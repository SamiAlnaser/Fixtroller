using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class editCreatedAtName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "TechnicianCategory",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "ProblemType",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "MaintenanceRequests",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceRequests");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TechnicianCategory",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ProblemType",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "MaintenanceRequests",
                newName: "CreatedAtUtc");
        }
    }
}
