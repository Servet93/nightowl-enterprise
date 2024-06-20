using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class verimortestcalls6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerimorTestCallsHistories");

            migrationBuilder.DropColumn(
                name: "VerimorCallFailed",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "VerimorCallId",
                table: "Invitations");

            migrationBuilder.CreateTable(
                name: "VoiceCallsHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    Pair = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CallDetailResult = table.Column<string>(type: "text", nullable: true),
                    SourceResult = table.Column<string>(type: "text", nullable: true),
                    DestinationResult = table.Column<string>(type: "text", nullable: true),
                    Ok = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceCallsHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoiceCallsHistories");

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

            migrationBuilder.CreateTable(
                name: "VerimorTestCallsHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallDetailResult = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    DestinationResult = table.Column<string>(type: "text", nullable: true),
                    Ok = table.Column<bool>(type: "boolean", nullable: true),
                    Pair = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    SourceResult = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerimorTestCallsHistories", x => x.Id);
                });
        }
    }
}
