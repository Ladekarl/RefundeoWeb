using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class TermsOfService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptedTermsOfService",
                table: "CustomerInformations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfService",
                table: "CustomerInformations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedTermsOfService",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "TermsOfService",
                table: "CustomerInformations");
        }
    }
}
