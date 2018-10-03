using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class PriceLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceLevel",
                table: "MerchantInformations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceLevel",
                table: "MerchantInformations");
        }
    }
}
