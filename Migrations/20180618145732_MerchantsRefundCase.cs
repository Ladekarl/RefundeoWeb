using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class MerchantsRefundCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "CustomerInformations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "CustomerInformations");
        }
    }
}
