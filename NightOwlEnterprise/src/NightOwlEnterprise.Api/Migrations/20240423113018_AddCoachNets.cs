using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachNets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoachDilNets",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    YDT = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachDilNets", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_CoachDilNets_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachMFNets",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: false),
                    Geometry = table.Column<byte>(type: "smallint", nullable: false),
                    Physics = table.Column<byte>(type: "smallint", nullable: false),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: false),
                    Biology = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachMFNets", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_CoachMFNets_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachSozelNets",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    History1 = table.Column<byte>(type: "smallint", nullable: false),
                    Geography1 = table.Column<byte>(type: "smallint", nullable: false),
                    Literature1 = table.Column<byte>(type: "smallint", nullable: false),
                    History2 = table.Column<byte>(type: "smallint", nullable: false),
                    Geography2 = table.Column<byte>(type: "smallint", nullable: false),
                    Philosophy = table.Column<byte>(type: "smallint", nullable: false),
                    Religion = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSozelNets", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_CoachSozelNets_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachTMNets",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: false),
                    Geometry = table.Column<byte>(type: "smallint", nullable: false),
                    Literature = table.Column<byte>(type: "smallint", nullable: false),
                    History = table.Column<byte>(type: "smallint", nullable: false),
                    Geography = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachTMNets", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_CoachTMNets_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachTYTNets",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Semantics = table.Column<byte>(type: "smallint", nullable: false),
                    Grammar = table.Column<byte>(type: "smallint", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: false),
                    Geometry = table.Column<byte>(type: "smallint", nullable: false),
                    History = table.Column<byte>(type: "smallint", nullable: false),
                    Geography = table.Column<byte>(type: "smallint", nullable: false),
                    Philosophy = table.Column<byte>(type: "smallint", nullable: false),
                    Religion = table.Column<byte>(type: "smallint", nullable: false),
                    Physics = table.Column<byte>(type: "smallint", nullable: false),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: false),
                    Biology = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachTYTNets", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_CoachTYTNets_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachDilNets");

            migrationBuilder.DropTable(
                name: "CoachMFNets");

            migrationBuilder.DropTable(
                name: "CoachSozelNets");

            migrationBuilder.DropTable(
                name: "CoachTMNets");

            migrationBuilder.DropTable(
                name: "CoachTYTNets");
        }
    }
}
