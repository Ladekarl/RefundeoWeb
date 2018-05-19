using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Refundeo.Migrations
{
    public partial class privacyPolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptedPrivacyPolicy",
                table: "CustomerInformations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy",
                table: "CustomerInformations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedPrivacyPolicy",
                table: "CustomerInformations");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicy",
                table: "CustomerInformations");
        }
    }
}
