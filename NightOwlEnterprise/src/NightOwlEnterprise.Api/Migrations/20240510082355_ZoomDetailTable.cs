using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ZoomDetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ZoomDetailId",
                table: "Invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CoachStudentTrainingSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ZoomMeetDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetId = table.Column<int>(type: "integer", nullable: true),
                    HostEmail = table.Column<string>(type: "text", nullable: true),
                    RegistrationUrl = table.Column<string>(type: "text", nullable: true),
                    JoinUrl = table.Column<string>(type: "text", nullable: true),
                    MeetingPasscode = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoachRegistrantId = table.Column<string>(type: "text", nullable: true),
                    CoachParticipantPinCode = table.Column<int>(type: "integer", nullable: false),
                    StudentRegistrantId = table.Column<string>(type: "text", nullable: true),
                    StudentParticipantPinCode = table.Column<int>(type: "integer", nullable: false),
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoomMeetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZoomMeetDetail_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZoomMeetDetail_InvitationId",
                table: "ZoomMeetDetail",
                column: "InvitationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZoomMeetDetail");

            migrationBuilder.DropColumn(
                name: "ZoomDetailId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CoachStudentTrainingSchedules");
        }
    }
}
