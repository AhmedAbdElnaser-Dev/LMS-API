using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class addLessonTranslationTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LessonTranslations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "LessonTranslations");
        }
    }
}
