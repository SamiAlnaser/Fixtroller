using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerAndClosedAtToMaintenanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAtUtc",
                table: "MaintenanceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_OwnerUserId",
                table: "MaintenanceRequests",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Users_OwnerUserId",
                table: "MaintenanceRequests",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Users_OwnerUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_OwnerUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "MaintenanceRequests");
        }
    }
}
