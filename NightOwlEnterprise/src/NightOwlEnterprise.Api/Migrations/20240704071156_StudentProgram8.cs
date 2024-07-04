using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class StudentProgram8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "StudentProgramDaily",
                newName: "CoachId");

            migrationBuilder.AddColumn<string>(
                name: "DateText",
                table: "StudentProgramDaily",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDaily_CoachId",
                table: "StudentProgramDaily",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgramDaily_Users_CoachId",
                table: "StudentProgramDaily",
                column: "CoachId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgramDaily_Users_CoachId",
                table: "StudentProgramDaily");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgramDaily_CoachId",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "DateText",
                table: "StudentProgramDaily");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "StudentProgramDaily",
                newName: "UserId");
        }
    }
}
