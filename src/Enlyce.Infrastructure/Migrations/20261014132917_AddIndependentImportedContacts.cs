using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndependentImportedContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_Email",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contacts",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AddColumn<bool>(
                name: "CanReceiveAutomatedEmail",
                table: "Contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Contacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(name: "CustomerType", table: "Contacts", type: "character varying(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CustomerStatus", table: "Contacts", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Nuevo");
            migrationBuilder.AddColumn<string>(name: "Address", table: "Contacts", type: "character varying(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Country", table: "Contacts", type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Colombia");
            migrationBuilder.AddColumn<string>(name: "Department", table: "Contacts", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "City", table: "Contacts", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerAdvisorId",
                table: "Contacts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email",
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_OwnerAdvisorId",
                table: "Contacts",
                column: "OwnerAdvisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Asesores_OwnerAdvisorId",
                table: "Contacts",
                column: "OwnerAdvisorId",
                principalTable: "Asesores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Asesores_OwnerAdvisorId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_Email",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_OwnerAdvisorId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CanReceiveAutomatedEmail",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Contacts");

            migrationBuilder.DropColumn(name: "CustomerType", table: "Contacts");
            migrationBuilder.DropColumn(name: "CustomerStatus", table: "Contacts");
            migrationBuilder.DropColumn(name: "Address", table: "Contacts");
            migrationBuilder.DropColumn(name: "Country", table: "Contacts");
            migrationBuilder.DropColumn(name: "Department", table: "Contacts");
            migrationBuilder.DropColumn(name: "City", table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OwnerAdvisorId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contacts",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email",
                unique: true);
        }
    }
}
