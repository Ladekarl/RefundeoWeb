using Microsoft.EntityFrameworkCore.Migrations;

namespace Refundeo.Migrations
{
    public partial class TermsVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrivacyPolicyVersion",
                table: "CustomerInformations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TermsOfServiceVersion",
                table: "CustomerInformations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyPolicyVersion",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "TermsOfServiceVersion",
                table: "CustomerInformations");
        }
    }
}
