using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewCoachDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ydt",
                table: "CoachDetail",
                newName: "Tyt");

            migrationBuilder.AddColumn<bool>(
                name: "ChangedSection",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "CoachDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FromSection",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Male",
                table: "CoachDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "Quota",
                table: "CoachDetail",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<long>(
                name: "Rank",
                table: "CoachDetail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ToSection",
                table: "CoachDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UniversityId",
                table: "CoachDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CoachDetail_DepartmentId",
                table: "CoachDetail",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachDetail_UniversityId",
                table: "CoachDetail",
                column: "UniversityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachDetail_Departments_DepartmentId",
                table: "CoachDetail",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachDetail_Universities_UniversityId",
                table: "CoachDetail",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachDetail_Departments_DepartmentId",
                table: "CoachDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachDetail_Universities_UniversityId",
                table: "CoachDetail");

            migrationBuilder.DropIndex(
                name: "IX_CoachDetail_DepartmentId",
                table: "CoachDetail");

            migrationBuilder.DropIndex(
                name: "IX_CoachDetail_UniversityId",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "ChangedSection",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "FromSection",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Male",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Quota",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "ToSection",
                table: "CoachDetail");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "CoachDetail");

            migrationBuilder.RenameColumn(
                name: "Tyt",
                table: "CoachDetail",
                newName: "Ydt");
        }
    }
}
