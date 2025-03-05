using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class addCoursesBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Courses_CourseId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_CourseId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "CoursesBooks",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursesBooks", x => new { x.CourseId, x.BookId });
                    table.ForeignKey(
                        name: "FK_CoursesBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursesBooks_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursesBooks_BookId",
                table: "CoursesBooks",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoursesBooks");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_CourseId",
                table: "Books",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Courses_CourseId",
                table: "Books",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
