using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DemandAndFirstResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrimerContacto",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Leads" AS lead
                SET "FechaPrimerContacto" = first_response."Fecha"
                FROM (
                    SELECT "LeadId", min("Fecha") AS "Fecha"
                    FROM "Interacciones"
                    WHERE "Tipo" <> 'ContactoWeb'
                    GROUP BY "LeadId"
                ) AS first_response
                WHERE first_response."LeadId" = lead."Id";
                """);

            migrationBuilder.CreateTable(
                name: "CustomerDemands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Operation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PropertyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MinimumPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaximumPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Bedrooms = table.Column<int>(type: "integer", nullable: true),
                    Bathrooms = table.Column<int>(type: "integer", nullable: true),
                    ParkingSpaces = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDemands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDemands_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerDemands_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DemandPropertyLinks",
                columns: table => new
                {
                    DemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandPropertyLinks", x => new { x.DemandId, x.PropertyId });
                    table.ForeignKey(
                        name: "FK_DemandPropertyLinks_CustomerDemands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "CustomerDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandPropertyLinks_Inmuebles_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Inmuebles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDemands_City_PropertyType_Operation",
                table: "CustomerDemands",
                columns: new[] { "City", "PropertyType", "Operation" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDemands_ContactId_Active",
                table: "CustomerDemands",
                columns: new[] { "ContactId", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDemands_LeadId",
                table: "CustomerDemands",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPropertyLinks_PropertyId",
                table: "DemandPropertyLinks",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemandPropertyLinks");

            migrationBuilder.DropTable(
                name: "CustomerDemands");

            migrationBuilder.DropColumn(
                name: "FechaPrimerContacto",
                table: "Leads");
        }
    }
}
