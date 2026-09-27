using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecureOwnerInquiryContinuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerInquiryTokenConsumedAt",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerInquiryTokenExpiresAt",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerInquiryTokenHash",
                table: "Leads",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerInquiryTokenHash",
                table: "Leads",
                column: "OwnerInquiryTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_OwnerInquiryTokenHash",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerInquiryTokenConsumedAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerInquiryTokenExpiresAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerInquiryTokenHash",
                table: "Leads");
        }
    }
}
