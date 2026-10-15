using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFunnelStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerStatus",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "CustomerFunnelStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FunnelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefaultForInbound = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFunnelStages", x => x.Id);
                });

            migrationBuilder.Sql("""
                WITH stage_pairs AS (
                    SELECT DISTINCT "CustomerFunnel" AS "FunnelName", "CustomerStatus" AS "Name"
                    FROM "Contacts"
                    WHERE "CustomerFunnel" IS NOT NULL AND "CustomerStatus" IS NOT NULL
                        AND length("CustomerFunnel") BETWEEN 1 AND 100 AND length("CustomerStatus") BETWEEN 1 AND 100
                    UNION SELECT 'Clientes', unnest(ARRAY['Nuevo', 'EnProceso', 'Convertido', 'Recuperado', 'Perdido']::text[])
                )
                INSERT INTO "CustomerFunnelStages" ("Id", "FunnelName", "Name", "SortOrder", "IsDefaultForInbound", "CreatedAtUtc")
                SELECT gen_random_uuid(), stage_pairs."FunnelName", stage_pairs."Name",
                       ROW_NUMBER() OVER (PARTITION BY stage_pairs."FunnelName" ORDER BY
                           CASE stage_pairs."Name"
                               WHEN 'Nuevo' THEN 0 WHEN 'EnProceso' THEN 1 WHEN 'Convertido' THEN 2
                               WHEN 'Recuperado' THEN 3 WHEN 'Perdido' THEN 4 ELSE 5
                           END, stage_pairs."Name") - 1,
                       stage_pairs."FunnelName" = 'Clientes' AND stage_pairs."Name" = 'Nuevo',
                       now()
                FROM stage_pairs
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFunnelStages_FunnelName_Name",
                table: "CustomerFunnelStages",
                columns: new[] { "FunnelName", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFunnelStages_IsDefaultForInbound",
                table: "CustomerFunnelStages",
                column: "IsDefaultForInbound",
                unique: true,
                filter: "\"IsDefaultForInbound\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFunnelStages_SortOrder",
                table: "CustomerFunnelStages",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFunnelStages");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerStatus",
                table: "Contacts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
