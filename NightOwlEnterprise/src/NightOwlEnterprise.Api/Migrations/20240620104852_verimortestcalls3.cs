using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class verimortestcalls3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationResult",
                table: "VerimorTestCallsHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceResult",
                table: "VerimorTestCallsHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationResult",
                table: "VerimorTestCallsHistories");

            migrationBuilder.DropColumn(
                name: "SourceResult",
                table: "VerimorTestCallsHistories");
        }
    }
}
