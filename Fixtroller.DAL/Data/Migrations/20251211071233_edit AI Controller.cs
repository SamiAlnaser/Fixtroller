using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class editAIController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "AiEmployeeChatSettings",
                newName: "IsTechnicianEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployeeEnabled",
                table: "AiEmployeeChatSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmployeeEnabled",
                table: "AiEmployeeChatSettings");

            migrationBuilder.RenameColumn(
                name: "IsTechnicianEnabled",
                table: "AiEmployeeChatSettings",
                newName: "IsEnabled");
        }
    }
}
