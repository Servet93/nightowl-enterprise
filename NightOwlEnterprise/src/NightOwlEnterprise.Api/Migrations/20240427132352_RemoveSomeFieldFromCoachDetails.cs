using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSomeFieldFromCoachDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dil",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Mf",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Sozel",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Tm",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Tyt",
                table: "CoachDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Dil",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Mf",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sozel",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Tm",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Tyt",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
