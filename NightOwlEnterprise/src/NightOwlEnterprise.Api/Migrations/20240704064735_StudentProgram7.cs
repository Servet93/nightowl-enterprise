using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class StudentProgram7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgramDaily_StudentPrograms_StudentProgramId",
                table: "StudentProgramDaily");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgramDaily_StudentProgramId",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "ExamType",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Excuse",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Lesson",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Not",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Resource",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "State",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "StudentProgramId",
                table: "StudentProgramDaily");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "StudentProgramDaily");

            migrationBuilder.CreateTable(
                name: "StudentProgramDailyTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProgramDailyId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    Lesson = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<byte>(type: "smallint", nullable: false),
                    Not = table.Column<string>(type: "text", nullable: false),
                    Excuse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgramDailyTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgramDailyTasks_StudentProgramDaily_StudentProgram~",
                        column: x => x.StudentProgramDailyId,
                        principalTable: "StudentProgramDaily",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDailyTasks_StudentProgramDailyId",
                table: "StudentProgramDailyTasks",
                column: "StudentProgramDailyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProgramDailyTasks");

            migrationBuilder.AddColumn<int>(
                name: "ExamType",
                table: "StudentProgramDaily",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Excuse",
                table: "StudentProgramDaily",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Lesson",
                table: "StudentProgramDaily",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "Minute",
                table: "StudentProgramDaily",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Not",
                table: "StudentProgramDaily",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "StudentProgramDaily",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Resource",
                table: "StudentProgramDaily",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "StudentProgramDaily",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentProgramId",
                table: "StudentProgramDaily",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "StudentProgramDaily",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDaily_StudentProgramId",
                table: "StudentProgramDaily",
                column: "StudentProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgramDaily_StudentPrograms_StudentProgramId",
                table: "StudentProgramDaily",
                column: "StudentProgramId",
                principalTable: "StudentPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
