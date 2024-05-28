using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class removegrammerandsemantics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grammar",
                table: "TYTNets");

            migrationBuilder.RenameColumn(
                name: "Semantics",
                table: "TYTNets",
                newName: "Turkish");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Turkish",
                table: "TYTNets",
                newName: "Semantics");

            migrationBuilder.AddColumn<byte>(
                name: "Grammar",
                table: "TYTNets",
                type: "smallint",
                nullable: true);
        }
    }
}
