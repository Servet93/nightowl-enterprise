using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCoachCalendarName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachCalendars_Users_CoachId",
                table: "CoachCalendars");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachCalendars_Users_StudentId",
                table: "CoachCalendars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoachCalendars",
                table: "CoachCalendars");

            migrationBuilder.RenameTable(
                name: "CoachCalendars",
                newName: "Invitations");

            migrationBuilder.RenameIndex(
                name: "IX_CoachCalendars_StudentId",
                table: "Invitations",
                newName: "IX_Invitations_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_CoachCalendars_CoachId",
                table: "Invitations",
                newName: "IX_Invitations_CoachId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invitations",
                table: "Invitations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Users_CoachId",
                table: "Invitations",
                column: "CoachId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Users_StudentId",
                table: "Invitations",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Users_CoachId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Users_StudentId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invitations",
                table: "Invitations");

            migrationBuilder.RenameTable(
                name: "Invitations",
                newName: "CoachCalendars");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_StudentId",
                table: "CoachCalendars",
                newName: "IX_CoachCalendars_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_CoachId",
                table: "CoachCalendars",
                newName: "IX_CoachCalendars_CoachId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoachCalendars",
                table: "CoachCalendars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachCalendars_Users_CoachId",
                table: "CoachCalendars",
                column: "CoachId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachCalendars_Users_StudentId",
                table: "CoachCalendars",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
