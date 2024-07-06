using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class StudentProgram12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Minute",
                table: "StudentProgramDailyTasks");

            migrationBuilder.AddColumn<int>(
                name: "CompletedMinute",
                table: "StudentProgramDailyTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DoneTime",
                table: "StudentProgramDailyTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinute",
                table: "StudentProgramDailyTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentProgramDailyTasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedMinute",
                table: "StudentProgramDailyTasks");

            migrationBuilder.DropColumn(
                name: "DoneTime",
                table: "StudentProgramDailyTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedMinute",
                table: "StudentProgramDailyTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentProgramDailyTasks");

            migrationBuilder.AddColumn<byte>(
                name: "Minute",
                table: "StudentProgramDailyTasks",
                type: "smallint",
                nullable: true);
        }
    }
}
