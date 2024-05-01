using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDetailsToSubsciptionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceId",
                table: "SubscriptionHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceState",
                table: "SubscriptionHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "SubscriptionHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionId",
                table: "SubscriptionHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionState",
                table: "SubscriptionHistories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "InvoiceState",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "SubscriptionState",
                table: "SubscriptionHistories");
        }
    }
}
