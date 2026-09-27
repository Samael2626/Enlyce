using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenSecurityAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpportunityKey",
                table: "Leads",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Leads"
                SET "OpportunityKey" = lower("Email") || '|' ||
                    COALESCE(replace("PublicationId"::text, '-', ''), 'general');

                WITH duplicates AS (
                    SELECT "Id",
                           row_number() OVER (
                               PARTITION BY "OpportunityKey"
                               ORDER BY "FechaCreacion", "Id") AS position
                    FROM "Leads"
                    WHERE "Activo" = TRUE
                )
                UPDATE "Leads" AS lead
                SET "Activo" = FALSE
                FROM duplicates
                WHERE lead."Id" = duplicates."Id" AND duplicates.position > 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OpportunityKey",
                table: "Leads",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionVersion",
                table: "Asesores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OpportunityKey",
                table: "Leads",
                column: "OpportunityKey",
                unique: true,
                filter: "\"Activo\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_OpportunityKey",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OpportunityKey",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SessionVersion",
                table: "Asesores");
        }
    }
}
