using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class ImageInContainers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefundCases_Documentations_DocumentationId",
                table: "RefundCases");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundCases_QRCodes_QRCodeId",
                table: "RefundCases");

            migrationBuilder.DropTable(
                name: "Documentations");

            migrationBuilder.DropTable(
                name: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_RefundCases_DocumentationId",
                table: "RefundCases");

            migrationBuilder.DropIndex(
                name: "IX_RefundCases_QRCodeId",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "DocumentationId",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "QRCodeId",
                table: "RefundCases");

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImage",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VATFormImage",
                table: "RefundCases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "ReceiptImage",
                table: "RefundCases");

            migrationBuilder.DropColumn(
                name: "VATFormImage",
                table: "RefundCases");

            migrationBuilder.AddColumn<long>(
                name: "DocumentationId",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QRCodeId",
                table: "RefundCases",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documentations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QRCodes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Image = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefundCases_DocumentationId",
                table: "RefundCases",
                column: "DocumentationId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundCases_QRCodeId",
                table: "RefundCases",
                column: "QRCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundCases_Documentations_DocumentationId",
                table: "RefundCases",
                column: "DocumentationId",
                principalTable: "Documentations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundCases_QRCodes_QRCodeId",
                table: "RefundCases",
                column: "QRCodeId",
                principalTable: "QRCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
