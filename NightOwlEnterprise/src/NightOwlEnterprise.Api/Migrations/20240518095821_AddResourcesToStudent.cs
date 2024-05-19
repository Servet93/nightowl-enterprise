using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcesToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourcesAYT",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<string>(type: "text", nullable: false),
                    Geometry = table.Column<string>(type: "text", nullable: false),
                    Physics = table.Column<string>(type: "text", nullable: false),
                    Chemistry = table.Column<string>(type: "text", nullable: false),
                    Biology = table.Column<string>(type: "text", nullable: false),
                    Literature = table.Column<string>(type: "text", nullable: false),
                    History = table.Column<string>(type: "text", nullable: false),
                    Geography = table.Column<string>(type: "text", nullable: false),
                    Philosophy = table.Column<string>(type: "text", nullable: false),
                    Religion = table.Column<string>(type: "text", nullable: false),
                    Turkish = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcesAYT", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ResourcesAYT_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourcesTYT",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Turkish = table.Column<string>(type: "text", nullable: false),
                    Mathematics = table.Column<string>(type: "text", nullable: false),
                    Geometry = table.Column<string>(type: "text", nullable: false),
                    History = table.Column<string>(type: "text", nullable: false),
                    Geography = table.Column<string>(type: "text", nullable: false),
                    Philosophy = table.Column<string>(type: "text", nullable: false),
                    Religion = table.Column<string>(type: "text", nullable: false),
                    Physics = table.Column<string>(type: "text", nullable: false),
                    Chemistry = table.Column<string>(type: "text", nullable: false),
                    Biology = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcesTYT", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ResourcesTYT_Users_UserId",
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
                name: "ResourcesAYT");

            migrationBuilder.DropTable(
                name: "ResourcesTYT");
        }
    }
}
