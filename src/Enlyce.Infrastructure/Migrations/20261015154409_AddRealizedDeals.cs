using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRealizedDeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealizedDeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdvisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ClosedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealizedDeals", x => x.Id);
                    table.CheckConstraint("CK_RealizedDeals_CommissionAmount", "\"CommissionAmount\" >= 0");
                    table.CheckConstraint("CK_RealizedDeals_Currency", "char_length(\"Currency\") = 3");
                    table.CheckConstraint("CK_RealizedDeals_GrossAmount", "\"GrossAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_RealizedDeals_Asesores_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealizedDeals_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealizedDeals_Inmuebles_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Inmuebles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealizedDeals_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RealizedDeals_AdvisorId",
                table: "RealizedDeals",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_RealizedDeals_ClosedOn",
                table: "RealizedDeals",
                column: "ClosedOn");

            migrationBuilder.CreateIndex(
                name: "IX_RealizedDeals_ContactId",
                table: "RealizedDeals",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_RealizedDeals_LeadId",
                table: "RealizedDeals",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_RealizedDeals_PropertyId",
                table: "RealizedDeals",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealizedDeals");
        }
    }
}
