using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class AdjustableRates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AdminAmount",
                table: "RefundCases",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MerchantAmount",
                table: "RefundCases",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VATAmount",
                table: "RefundCases",
                nullable: false,
                defaultValue: 0.0);

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
                name: "VATRate",
                table: "MerchantInformations",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminAmount",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "MerchantAmount",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "VATAmount",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "AdminFee",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "MerchantFee",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "MerchantInformations");
        }
    }
}
