using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class GooglePlacesId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GooglePlaceId",
                table: "Cities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GooglePlaceId",
                table: "Cities");
        }
    }
}
