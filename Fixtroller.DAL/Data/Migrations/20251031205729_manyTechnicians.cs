using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class manyTechnicians : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Users_AssignedTechnicianUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_AssignedTechnicianUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedAtUtc",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianUserId",
                table: "MaintenanceRequests");

            migrationBuilder.CreateTable(
                name: "MaintenanceRequestTechnician",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    TechnicianUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnassignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRequestTechnician", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequestTechnician_MaintenanceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequestTechnician_RequestId_TechnicianUserId",
                table: "MaintenanceRequestTechnician",
                columns: new[] { "RequestId", "TechnicianUserId" },
                unique: true,
                filter: "[UnassignedAtUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceRequestTechnician");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAtUtc",
                table: "MaintenanceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTechnicianUserId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_AssignedTechnicianUserId",
                table: "MaintenanceRequests",
                column: "AssignedTechnicianUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Users_AssignedTechnicianUserId",
                table: "MaintenanceRequests",
                column: "AssignedTechnicianUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
