using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class Vat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VATNumber",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CustomerInformations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "VATNumber",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CustomerInformations");
        }
    }
}
