using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupsTranslations_Groups_GroupId1",
                table: "GroupsTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsTranslations",
                table: "GroupsTranslations");

            migrationBuilder.DropIndex(
                name: "IX_GroupsTranslations_GroupId1",
                table: "GroupsTranslations");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "GroupsTranslations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsTranslations",
                table: "GroupsTranslations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsTranslations_GroupId_Language",
                table: "GroupsTranslations",
                columns: new[] { "GroupId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsTranslations",
                table: "GroupsTranslations");

            migrationBuilder.DropIndex(
                name: "IX_GroupsTranslations_GroupId_Language",
                table: "GroupsTranslations");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId1",
                table: "GroupsTranslations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsTranslations",
                table: "GroupsTranslations",
                columns: new[] { "GroupId", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupsTranslations_GroupId1",
                table: "GroupsTranslations",
                column: "GroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsTranslations_Groups_GroupId1",
                table: "GroupsTranslations",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
