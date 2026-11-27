using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistico.Modules.Shipments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CityPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "destination_city_code",
                schema: "shipment",
                table: "shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cities",
                schema: "shipment",
                table: "rate_cards",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "destination_city_code",
                schema: "shipment",
                table: "shipments");

            migrationBuilder.DropColumn(
                name: "cities",
                schema: "shipment",
                table: "rate_cards");
        }
    }
}
