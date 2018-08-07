using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class FeePoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminFee",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "MerchantFee",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "RefundPercentage",
                table: "MerchantInformations");

            migrationBuilder.CreateTable(
                name: "FeePoints",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MerchantFee = table.Column<double>(nullable: false),
                    AdminFee = table.Column<double>(nullable: false),
                    RefundPercentage = table.Column<double>(nullable: false),
                    Start = table.Column<double>(nullable: false),
                    End = table.Column<double>(nullable: true),
                    MerchantInformationId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePoints_MerchantInformations_MerchantInformationId",
                        column: x => x.MerchantInformationId,
                        principalTable: "MerchantInformations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeePoints_MerchantInformationId",
                table: "FeePoints",
                column: "MerchantInformationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeePoints");

            migrationBuilder.AddColumn<double>(
                name: "AdminFee",
                table: "MerchantInformations",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MerchantFee",
                table: "MerchantInformations",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RefundPercentage",
                table: "MerchantInformations",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
