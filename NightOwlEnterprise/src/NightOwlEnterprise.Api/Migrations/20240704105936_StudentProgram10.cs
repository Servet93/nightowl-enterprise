using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class StudentProgram10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskType",
                table: "StudentProgramDailyTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "StudentProgramDailyTasks");
        }
    }
}
