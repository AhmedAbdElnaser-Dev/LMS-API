using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class addLessonDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "Units");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Lessons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
