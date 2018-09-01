using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class CityLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LocationId",
                table: "Cities",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_LocationId",
                table: "Cities",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Locations_LocationId",
                table: "Cities",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Locations_LocationId",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Cities_LocationId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Cities");
        }
    }
}
