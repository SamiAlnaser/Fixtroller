using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditTheNamesForDatabaes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PTypes",
                table: "PTypes");

            migrationBuilder.RenameTable(
                name: "PTypes",
                newName: "ProblemType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProblemType",
                table: "ProblemType",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProblemType",
                table: "ProblemType");

            migrationBuilder.RenameTable(
                name: "ProblemType",
                newName: "PTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PTypes",
                table: "PTypes",
                column: "Id");
        }
    }
}
