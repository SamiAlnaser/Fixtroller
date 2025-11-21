using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedDurationToMaintenanceRequestTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpectedDuration",
                table: "MaintenanceRequestTechnician",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedDuration",
                table: "MaintenanceRequestTechnician");
        }
    }
}
