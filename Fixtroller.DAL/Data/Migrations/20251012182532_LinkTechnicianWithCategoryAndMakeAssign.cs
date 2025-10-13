using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkTechnicianWithCategoryAndMakeAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TechnicianCategoryId",
                table: "Users",
                type: "int",
                nullable: true);

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
                name: "IX_Users_TechnicianCategoryId",
                table: "Users",
                column: "TechnicianCategoryId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TechnicianCategory_TechnicianCategoryId",
                table: "Users",
                column: "TechnicianCategoryId",
                principalTable: "TechnicianCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Users_AssignedTechnicianUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_TechnicianCategory_TechnicianCategoryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TechnicianCategoryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_AssignedTechnicianUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "TechnicianCategoryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedAtUtc",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianUserId",
                table: "MaintenanceRequests");
        }
    }
}
