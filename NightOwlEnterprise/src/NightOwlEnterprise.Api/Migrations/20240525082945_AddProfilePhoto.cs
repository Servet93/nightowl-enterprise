using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfilePhotos",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Photo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePhotos", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ProfilePhotos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfilePhotos");
        }
    }
}
