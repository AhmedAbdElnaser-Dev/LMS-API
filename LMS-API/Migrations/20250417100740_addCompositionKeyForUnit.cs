using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class addCompositionKeyForUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_Name",
                table: "Units");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name_CourseId",
                table: "Units",
                columns: new[] { "Name", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_Name_CourseId",
                table: "Units");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name",
                unique: true);
        }
    }
}
