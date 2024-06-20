using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class verimor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerimorCallFailed",
                table: "Invitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerimorCallId",
                table: "Invitations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerimorCallFailed",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "VerimorCallId",
                table: "Invitations");
        }
    }
}
