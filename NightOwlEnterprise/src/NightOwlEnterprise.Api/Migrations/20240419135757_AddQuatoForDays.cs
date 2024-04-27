using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuatoForDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quota",
                table: "CoachDetail",
                newName: "WednesdayQuota");

            migrationBuilder.AddColumn<byte>(
                name: "FridayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "MondayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SaturdayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "StudentQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SundayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ThursdayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "TuesdayQuota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "MondayQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "SaturdayQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "StudentQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "SundayQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "ThursdayQuota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "TuesdayQuota",
                table: "CoachDetail");

            migrationBuilder.RenameColumn(
                name: "WednesdayQuota",
                table: "CoachDetail",
                newName: "Quota");
        }
    }
}
