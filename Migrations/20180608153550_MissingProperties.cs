using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class MissingProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BankRegNumber",
                table: "CustomerInformations",
                newName: "Swift");

            migrationBuilder.RenameColumn(
                name: "BankAccountNumber",
                table: "CustomerInformations",
                newName: "Passport");

            migrationBuilder.AlterColumn<double>(
                name: "RefundPercentage",
                table: "MerchantInformations",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LocationId",
                table: "MerchantInformations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "CustomerInformations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CustomerInformations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    StreetName = table.Column<string>(nullable: true),
                    StreetNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInformations_AddressId",
                table: "MerchantInformations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInformations_LocationId",
                table: "MerchantInformations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInformations_AddressId",
                table: "CustomerInformations",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInformations_Addresses_AddressId",
                table: "CustomerInformations",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInformations_Addresses_AddressId",
                table: "MerchantInformations",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInformations_Locations_LocationId",
                table: "MerchantInformations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInformations_Addresses_AddressId",
                table: "CustomerInformations");

            migrationBuilder.DropForeignKey(
                name: "FK_MerchantInformations_Addresses_AddressId",
                table: "MerchantInformations");

            migrationBuilder.DropForeignKey(
                name: "FK_MerchantInformations_Locations_LocationId",
                table: "MerchantInformations");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_MerchantInformations_AddressId",
                table: "MerchantInformations");

            migrationBuilder.DropIndex(
                name: "IX_MerchantInformations_LocationId",
                table: "MerchantInformations");

            migrationBuilder.DropIndex(
                name: "IX_CustomerInformations_AddressId",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "MerchantInformations");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CustomerInformations");

            migrationBuilder.RenameColumn(
                name: "Swift",
                table: "CustomerInformations",
                newName: "BankRegNumber");

            migrationBuilder.RenameColumn(
                name: "Passport",
                table: "CustomerInformations",
                newName: "BankAccountNumber");

            migrationBuilder.AlterColumn<int>(
                name: "RefundPercentage",
                table: "MerchantInformations",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
