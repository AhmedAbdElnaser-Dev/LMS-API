using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class compositionKeyForUnitTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitTranslations_UnitId",
                table: "UnitTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UnitTranslations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTranslations_UnitId_Language_Name",
                table: "UnitTranslations",
                columns: new[] { "UnitId", "Language", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitTranslations_UnitId_Language_Name",
                table: "UnitTranslations");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Units");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UnitTranslations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTranslations_UnitId",
                table: "UnitTranslations",
                column: "UnitId");
        }
    }
}
