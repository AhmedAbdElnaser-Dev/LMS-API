using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToBookTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "BookTranslations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslations_Name",
                table: "BookTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslations_Name_Language_BookId",
                table: "BookTranslations",
                columns: new[] { "Name", "Language", "BookId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookTranslations_Name",
                table: "BookTranslations");

            migrationBuilder.DropIndex(
                name: "IX_BookTranslations_Name_Language_BookId",
                table: "BookTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "BookTranslations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
