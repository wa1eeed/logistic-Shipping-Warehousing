using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistico.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class BrandColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "brand_color",
                schema: "iam",
                table: "company_profiles",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "brand_color",
                schema: "iam",
                table: "company_profiles");
        }
    }
}
