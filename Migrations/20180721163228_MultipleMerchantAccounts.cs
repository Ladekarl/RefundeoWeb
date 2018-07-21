using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class MultipleMerchantAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MerchantInformationId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MerchantInformationId",
                table: "AspNetUsers",
                column: "MerchantInformationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MerchantInformationId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MerchantInformationId",
                table: "AspNetUsers",
                column: "MerchantInformationId",
                unique: true,
                filter: "[MerchantInformationId] IS NOT NULL");
        }
    }
}
