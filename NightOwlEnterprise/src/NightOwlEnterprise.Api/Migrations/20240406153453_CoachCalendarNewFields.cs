using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class CoachCalendarNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TM",
                table: "CoachDetail",
                newName: "Tm");

            migrationBuilder.RenameColumn(
                name: "MF",
                table: "CoachDetail",
                newName: "Mf");

            migrationBuilder.AddColumn<bool>(
                name: "Dil",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dil",
                table: "CoachDetail");

            migrationBuilder.RenameColumn(
                name: "Tm",
                table: "CoachDetail",
                newName: "TM");

            migrationBuilder.RenameColumn(
                name: "Mf",
                table: "CoachDetail",
                newName: "MF");
        }
    }
}
