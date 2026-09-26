using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerInquiryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OwnerExpectedPrice",
                table: "Leads",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPreferredContactChannel",
                table: "Leads",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPropertyCity",
                table: "Leads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPropertyMessage",
                table: "Leads",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPropertyNeighborhood",
                table: "Leads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPropertyType",
                table: "Leads",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerPropertyCity",
                table: "Leads",
                column: "OwnerPropertyCity");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerPropertyType",
                table: "Leads",
                column: "OwnerPropertyType");

            migrationBuilder.Sql(
                """
                UPDATE "Leads"
                SET "OwnerService" = 'Valuation'
                WHERE "OwnerService" IS NULL
                  AND "Fuente" LIKE 'PropietarioWeb:Avaluar%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Leads"
                SET "OwnerService" = NULL
                WHERE "OwnerService" = 'Valuation'
                  AND "Fuente" LIKE 'PropietarioWeb:Avaluar%';
                """);

            migrationBuilder.DropIndex(
                name: "IX_Leads_OwnerPropertyCity",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_OwnerPropertyType",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerExpectedPrice",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerPreferredContactChannel",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerPropertyCity",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerPropertyMessage",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerPropertyNeighborhood",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerPropertyType",
                table: "Leads");
        }
    }
}
