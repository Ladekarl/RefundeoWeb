using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class dates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInformations_AspNetUsers_CustomerId",
                table: "CustomerInformations");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "RefundCases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRequested",
                table: "RefundCases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInformations_AspNetUsers_CustomerId",
                table: "CustomerInformations",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInformations_AspNetUsers_CustomerId",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "DateRequested",
                table: "RefundCases");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInformations_AspNetUsers_CustomerId",
                table: "CustomerInformations",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
