using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateTutoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrivateTutoring",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "School",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PrivateTutoringDil",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    YTD = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateTutoringDil", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_PrivateTutoringDil_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateTutoringMF",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<bool>(type: "boolean", nullable: false),
                    Geometry = table.Column<bool>(type: "boolean", nullable: false),
                    Physics = table.Column<bool>(type: "boolean", nullable: false),
                    Chemistry = table.Column<bool>(type: "boolean", nullable: false),
                    Biology = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateTutoringMF", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_PrivateTutoringMF_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateTutoringSozel",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    History1 = table.Column<bool>(type: "boolean", nullable: false),
                    Geography1 = table.Column<bool>(type: "boolean", nullable: false),
                    Literature1 = table.Column<bool>(type: "boolean", nullable: false),
                    History2 = table.Column<bool>(type: "boolean", nullable: false),
                    Geography2 = table.Column<bool>(type: "boolean", nullable: false),
                    Philosophy = table.Column<bool>(type: "boolean", nullable: false),
                    Religion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateTutoringSozel", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_PrivateTutoringSozel_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateTutoringTM",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<bool>(type: "boolean", nullable: false),
                    Geometry = table.Column<bool>(type: "boolean", nullable: false),
                    Literature = table.Column<bool>(type: "boolean", nullable: false),
                    History = table.Column<bool>(type: "boolean", nullable: false),
                    Geography = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateTutoringTM", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_PrivateTutoringTM_Users_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateTutoringTYT",
                columns: table => new
                {
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Turkish = table.Column<bool>(type: "boolean", nullable: false),
                    Mathematics = table.Column<bool>(type: "boolean", nullable: false),
                    Geometry = table.Column<bool>(type: "boolean", nullable: false),
                    History = table.Column<bool>(type: "boolean", nullable: false),
                    Geography = table.Column<bool>(type: "boolean", nullable: false),
                    Philosophy = table.Column<bool>(type: "boolean", nullable: false),
                    Religion = table.Column<bool>(type: "boolean", nullable: false),
                    Physics = table.Column<bool>(type: "boolean", nullable: false),
                    Chemistry = table.Column<bool>(type: "boolean", nullable: false),
                    Biology = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateTutoringTYT", x => x.CoachId);
                    table.ForeignKey(
                        name: "FK_PrivateTutoringTYT_Users_CoachId",
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
                name: "PrivateTutoringDil");

            migrationBuilder.DropTable(
                name: "PrivateTutoringMF");

            migrationBuilder.DropTable(
                name: "PrivateTutoringSozel");

            migrationBuilder.DropTable(
                name: "PrivateTutoringTM");

            migrationBuilder.DropTable(
                name: "PrivateTutoringTYT");

            migrationBuilder.DropColumn(
                name: "PrivateTutoring",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "School",
                table: "CoachDetail");
        }
    }
}
