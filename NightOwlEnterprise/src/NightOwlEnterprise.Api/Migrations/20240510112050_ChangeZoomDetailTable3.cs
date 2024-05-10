using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeZoomDetailTable3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoachJoinUrl",
                table: "ZoomMeetDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentJoinUrl",
                table: "ZoomMeetDetail",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoachJoinUrl",
                table: "ZoomMeetDetail");

            migrationBuilder.DropColumn(
                name: "StudentJoinUrl",
                table: "ZoomMeetDetail");
        }
    }
}
