using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalInfoToCoachDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromSection",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "ToSection",
                table: "CoachDetail");

            migrationBuilder.RenameColumn(
                name: "ChangedSection",
                table: "CoachDetail",
                newName: "ChangedDepartmentType");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "CoachDetail",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DepartmentType",
                table: "CoachDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "FirstAytNet",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "FromDepartment",
                table: "CoachDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HighSchool",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "HighSchoolGPA",
                table: "CoachDetail",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<byte>(
                name: "LastAytNet",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "LastTytNet",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ToDepartment",
                table: "CoachDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoachYksRankings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<string>(type: "text", nullable: false),
                    Enter = table.Column<bool>(type: "boolean", nullable: false),
                    Rank = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachYksRankings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachYksRankings_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachYksRankings_CoachId",
                table: "CoachYksRankings",
                column: "CoachId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachYksRankings");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "DepartmentType",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "FirstAytNet",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "FromDepartment",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "HighSchool",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "HighSchoolGPA",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "LastAytNet",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "LastTytNet",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "ToDepartment",
                table: "CoachDetail");

            migrationBuilder.RenameColumn(
                name: "ChangedDepartmentType",
                table: "CoachDetail",
                newName: "ChangedSection");

            migrationBuilder.AddColumn<string>(
                name: "FromSection",
                table: "CoachDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToSection",
                table: "CoachDetail",
                type: "text",
                nullable: true);
        }
    }
}
