using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNetsTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachTYTNets");

            migrationBuilder.AddColumn<int>(
                name: "ExamType",
                table: "StudentDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "StudentDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TYTNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Semantics = table.Column<byte>(type: "smallint", nullable: true),
                    Grammar = table.Column<byte>(type: "smallint", nullable: true),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: true),
                    Geometry = table.Column<byte>(type: "smallint", nullable: true),
                    History = table.Column<byte>(type: "smallint", nullable: true),
                    Geography = table.Column<byte>(type: "smallint", nullable: true),
                    Philosophy = table.Column<byte>(type: "smallint", nullable: true),
                    Religion = table.Column<byte>(type: "smallint", nullable: true),
                    Physics = table.Column<byte>(type: "smallint", nullable: true),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: true),
                    Biology = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TYTNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TYTNets_Users_UserId",
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
                name: "TYTNets");

            migrationBuilder.DropColumn(
                name: "ExamType",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StudentDetail");

            migrationBuilder.CreateTable(
                name: "CoachTYTNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Biology = table.Column<byte>(type: "smallint", nullable: true),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: true),
                    Geography = table.Column<byte>(type: "smallint", nullable: true),
                    Geometry = table.Column<byte>(type: "smallint", nullable: true),
                    Grammar = table.Column<byte>(type: "smallint", nullable: true),
                    History = table.Column<byte>(type: "smallint", nullable: true),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: true),
                    Philosophy = table.Column<byte>(type: "smallint", nullable: true),
                    Physics = table.Column<byte>(type: "smallint", nullable: true),
                    Religion = table.Column<byte>(type: "smallint", nullable: true),
                    Semantics = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachTYTNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_CoachTYTNets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
