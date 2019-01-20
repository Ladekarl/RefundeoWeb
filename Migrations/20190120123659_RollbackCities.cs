using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class RollbackCities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantInformations_Cities_CityId",
                table: "MerchantInformations");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_MerchantInformations_CityId",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "MerchantInformations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CityId",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GooglePlaceId = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    LocationId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInformations_CityId",
                table: "MerchantInformations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_LocationId",
                table: "Cities",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInformations_Cities_CityId",
                table: "MerchantInformations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
