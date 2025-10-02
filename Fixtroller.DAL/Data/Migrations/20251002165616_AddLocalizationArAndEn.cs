using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixtroller.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationArAndEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "TechnicianCategory");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProblemType");

            migrationBuilder.CreateTable(
                name: "ProblemTypeTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblemTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemTypeTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemTypeTranslation_ProblemType_ProblemTypeId",
                        column: x => x.ProblemTypeId,
                        principalTable: "ProblemType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianCategoryTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechnicianCategoryyId = table.Column<int>(type: "int", nullable: false),
                    TechnicianCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianCategoryTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicianCategoryTranslation_TechnicianCategory_TechnicianCategoryId",
                        column: x => x.TechnicianCategoryId,
                        principalTable: "TechnicianCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProblemTypeTranslation_ProblemTypeId",
                table: "ProblemTypeTranslation",
                column: "ProblemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianCategoryTranslation_TechnicianCategoryId",
                table: "TechnicianCategoryTranslation",
                column: "TechnicianCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProblemTypeTranslation");

            migrationBuilder.DropTable(
                name: "TechnicianCategoryTranslation");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TechnicianCategory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProblemType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
