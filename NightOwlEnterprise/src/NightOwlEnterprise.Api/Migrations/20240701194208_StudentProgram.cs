using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class StudentProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPrograms_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPrograms_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgramWeekly",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDateText = table.Column<string>(type: "text", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateText = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgramWeekly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgramWeekly_StudentPrograms_StudentProgramId",
                        column: x => x.StudentProgramId,
                        principalTable: "StudentPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgramWeekly_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgramWeekly_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgramDaily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProgramWeeklyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    DayText = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    Lesson = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<byte>(type: "smallint", nullable: false),
                    Not = table.Column<string>(type: "text", nullable: false),
                    Excuse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgramDaily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgramDaily_StudentProgramWeekly_StudentProgramWeek~",
                        column: x => x.StudentProgramWeeklyId,
                        principalTable: "StudentProgramWeekly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgramDaily_StudentPrograms_StudentProgramId",
                        column: x => x.StudentProgramId,
                        principalTable: "StudentPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgramDaily_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDaily_StudentId",
                table: "StudentProgramDaily",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDaily_StudentProgramId",
                table: "StudentProgramDaily",
                column: "StudentProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramDaily_StudentProgramWeeklyId",
                table: "StudentProgramDaily",
                column: "StudentProgramWeeklyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPrograms_CoachId",
                table: "StudentPrograms",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPrograms_StudentId",
                table: "StudentPrograms",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramWeekly_CoachId",
                table: "StudentProgramWeekly",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramWeekly_StudentId",
                table: "StudentProgramWeekly",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgramWeekly_StudentProgramId",
                table: "StudentProgramWeekly",
                column: "StudentProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProgramDaily");

            migrationBuilder.DropTable(
                name: "StudentProgramWeekly");

            migrationBuilder.DropTable(
                name: "StudentPrograms");
        }
    }
}
