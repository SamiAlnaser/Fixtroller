using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class putLeafletjs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "MaintenanceRequests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "MaintenanceRequests",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "MaintenanceRequests",
                type: "decimal(9,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "MaintenanceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "MaintenanceRequests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
