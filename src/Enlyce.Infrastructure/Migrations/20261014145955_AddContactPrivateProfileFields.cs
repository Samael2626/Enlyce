using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPrivateProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcquisitionMedium",
                table: "Contacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalData",
                table: "Contacts",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Contacts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationNumber",
                table: "Contacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landline",
                table: "Contacts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredBy",
                table: "Contacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcquisitionMedium",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "AdditionalData",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IdentificationNumber",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Landline",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ReferredBy",
                table: "Contacts");
        }
    }
}
