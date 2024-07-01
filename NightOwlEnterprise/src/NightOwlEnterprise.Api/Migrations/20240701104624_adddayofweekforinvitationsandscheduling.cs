using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class adddayofweekforinvitationsandscheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Day",
                table: "CoachStudentTrainingSchedules",
                newName: "VoiceDay");

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "Invitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VideoDay",
                table: "CoachStudentTrainingSchedules",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "VideoDay",
                table: "CoachStudentTrainingSchedules");

            migrationBuilder.RenameColumn(
                name: "VoiceDay",
                table: "CoachStudentTrainingSchedules",
                newName: "Day");
        }
    }
}
