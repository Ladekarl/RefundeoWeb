using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class City : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInformations_CityId",
                table: "MerchantInformations",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInformations_Cities_CityId",
                table: "MerchantInformations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
