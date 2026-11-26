using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistico.Modules.Merchants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MerchantSignupFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "daily_shipments",
                schema: "merchant",
                table: "merchants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "self_registered",
                schema: "merchant",
                table: "merchants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_shipments",
                schema: "merchant",
                table: "merchants");

            migrationBuilder.DropColumn(
                name: "self_registered",
                schema: "merchant",
                table: "merchants");
        }
    }
}
