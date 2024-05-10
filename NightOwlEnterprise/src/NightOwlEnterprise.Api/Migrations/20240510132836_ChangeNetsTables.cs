using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNetsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachTYTNets_Users_CoachId",
                table: "CoachTYTNets");

            migrationBuilder.DropTable(
                name: "CoachDilNets");

            migrationBuilder.DropTable(
                name: "CoachMFNets");

            migrationBuilder.DropTable(
                name: "CoachSozelNets");

            migrationBuilder.DropTable(
                name: "CoachTMNets");

            migrationBuilder.DropColumn(
                name: "TermsAndConditionsAccepted",
                table: "StudentDetail");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "CoachTYTNets",
                newName: "UserId");

            migrationBuilder.AddColumn<byte>(
                name: "AytGoalNet",
                table: "StudentDetail",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Course",
                table: "StudentDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesiredProfessionSchoolField",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GoalRanking",
                table: "StudentDetail",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighSchool",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HighSchoolGPA",
                table: "StudentDetail",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentMobile",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentSurname",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivateTutoringAyt",
                table: "StudentDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivateTutoringTyt",
                table: "StudentDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "School",
                table: "StudentDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "StudentDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TytGoalNet",
                table: "StudentDetail",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Youtube",
                table: "StudentDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Semantics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Religion",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Physics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Philosophy",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Mathematics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "History",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Grammar",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Geometry",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Geography",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Chemistry",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "Biology",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.CreateTable(
                name: "DilNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    YDT = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DilNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_DilNets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MFNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: true),
                    Geometry = table.Column<byte>(type: "smallint", nullable: true),
                    Physics = table.Column<byte>(type: "smallint", nullable: true),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: true),
                    Biology = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_MFNets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SozelNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    History1 = table.Column<byte>(type: "smallint", nullable: true),
                    Geography1 = table.Column<byte>(type: "smallint", nullable: true),
                    Literature1 = table.Column<byte>(type: "smallint", nullable: true),
                    History2 = table.Column<byte>(type: "smallint", nullable: true),
                    Geography2 = table.Column<byte>(type: "smallint", nullable: true),
                    Philosophy = table.Column<byte>(type: "smallint", nullable: true),
                    Religion = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SozelNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_SozelNets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TMNets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: true),
                    Geometry = table.Column<byte>(type: "smallint", nullable: true),
                    Literature = table.Column<byte>(type: "smallint", nullable: true),
                    History = table.Column<byte>(type: "smallint", nullable: true),
                    Geography = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMNets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TMNets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CoachTYTNets_Users_UserId",
                table: "CoachTYTNets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachTYTNets_Users_UserId",
                table: "CoachTYTNets");

            migrationBuilder.DropTable(
                name: "DilNets");

            migrationBuilder.DropTable(
                name: "MFNets");

            migrationBuilder.DropTable(
                name: "SozelNets");

            migrationBuilder.DropTable(
                name: "TMNets");

            migrationBuilder.DropColumn(
                name: "AytGoalNet",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Course",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "DesiredProfessionSchoolField",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "GoalRanking",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "HighSchool",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "HighSchoolGPA",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "ParentMobile",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "ParentSurname",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "PrivateTutoringAyt",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "PrivateTutoringTyt",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "School",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "TytGoalNet",
                table: "StudentDetail");

            migrationBuilder.DropColumn(
                name: "Youtube",
                table: "StudentDetail");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CoachTYTNets",
                newName: "CoachId");

            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditionsAccepted",
                table: "StudentDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<byte>(
                name: "Semantics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Religion",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Physics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Philosophy",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Mathematics",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "History",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Grammar",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Geometry",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Geography",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Chemistry",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Biology",
                table: "CoachTYTNets",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

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
                    Biology = table.Column<byte>(type: "smallint", nullable: false),
                    Chemistry = table.Column<byte>(type: "smallint", nullable: false),
                    Geometry = table.Column<byte>(type: "smallint", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: false),
                    Physics = table.Column<byte>(type: "smallint", nullable: false)
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
                    Geography1 = table.Column<byte>(type: "smallint", nullable: false),
                    Geography2 = table.Column<byte>(type: "smallint", nullable: false),
                    History1 = table.Column<byte>(type: "smallint", nullable: false),
                    History2 = table.Column<byte>(type: "smallint", nullable: false),
                    Literature1 = table.Column<byte>(type: "smallint", nullable: false),
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
                    Geography = table.Column<byte>(type: "smallint", nullable: false),
                    Geometry = table.Column<byte>(type: "smallint", nullable: false),
                    History = table.Column<byte>(type: "smallint", nullable: false),
                    Literature = table.Column<byte>(type: "smallint", nullable: false),
                    Mathematics = table.Column<byte>(type: "smallint", nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_CoachTYTNets_Users_CoachId",
                table: "CoachTYTNets",
                column: "CoachId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
