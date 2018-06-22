using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class signatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerSignature",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantSignature",
                table: "RefundCases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerSignature",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "MerchantSignature",
                table: "RefundCases");
        }
    }
}
