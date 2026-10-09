using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkLeadsToContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Contacts" ("Id", "Name", "Email", "Phone", "CreatedAt", "UpdatedAt", "Active")
                SELECT gen_random_uuid(),
                       (ARRAY_AGG("Nombre" ORDER BY "FechaCreacion"))[1],
                       lower(trim("Email")),
                       (ARRAY_AGG("Telefono" ORDER BY "FechaCreacion"))[1],
                       min("FechaCreacion"),
                       max("FechaActualizacion"),
                       bool_or("Activo")
                FROM "Leads"
                GROUP BY lower(trim("Email"));

                UPDATE "Leads" AS lead
                SET "ContactId" = contact."Id"
                FROM "Contacts" AS contact
                WHERE lower(trim(lead."Email")) = contact."Email";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ContactId",
                table: "Leads",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Contacts_ContactId",
                table: "Leads",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Contacts_ContactId",
                table: "Leads");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ContactId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Leads");
        }
    }
}
