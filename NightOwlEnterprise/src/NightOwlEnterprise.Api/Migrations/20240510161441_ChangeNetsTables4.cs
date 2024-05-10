using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNetsTables4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZoomMeetDetail_Invitations_InvitationId",
                table: "ZoomMeetDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZoomMeetDetail",
                table: "ZoomMeetDetail");

            migrationBuilder.DropIndex(
                name: "IX_ZoomMeetDetail_InvitationId",
                table: "ZoomMeetDetail");

            migrationBuilder.RenameTable(
                name: "ZoomMeetDetail",
                newName: "ZoomMeetDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZoomMeetDetails",
                table: "ZoomMeetDetails",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ZoomMeetDetailId",
                table: "Invitations",
                column: "ZoomMeetDetailId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_ZoomMeetDetails_ZoomMeetDetailId",
                table: "Invitations",
                column: "ZoomMeetDetailId",
                principalTable: "ZoomMeetDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_ZoomMeetDetails_ZoomMeetDetailId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_ZoomMeetDetailId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZoomMeetDetails",
                table: "ZoomMeetDetails");

            migrationBuilder.RenameTable(
                name: "ZoomMeetDetails",
                newName: "ZoomMeetDetail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZoomMeetDetail",
                table: "ZoomMeetDetail",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ZoomMeetDetail_InvitationId",
                table: "ZoomMeetDetail",
                column: "InvitationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ZoomMeetDetail_Invitations_InvitationId",
                table: "ZoomMeetDetail",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
