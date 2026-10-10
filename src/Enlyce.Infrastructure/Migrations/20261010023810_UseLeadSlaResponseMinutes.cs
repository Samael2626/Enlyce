using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseLeadSlaResponseMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstResponseHours",
                table: "LeadSlaRules",
                newName: "FirstResponseMinutes");

            migrationBuilder.Sql("UPDATE \"LeadSlaRules\" SET \"FirstResponseMinutes\" = 45;");
            migrationBuilder.Sql("""
                INSERT INTO "LeadSlaRules"
                    ("Id", "SourceKey", "OperationType", "FirstResponseMinutes", "InactivityDays", "Enabled", "UpdatedAtUtc")
                SELECT '6e8d8f7e-9f7c-4a8e-8b06-6d66c5d8e045', '*', '*', 45, NULL, TRUE, CURRENT_TIMESTAMP
                WHERE NOT EXISTS (
                    SELECT 1 FROM "LeadSlaRules" WHERE "SourceKey" = '*' AND "OperationType" = '*'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"LeadSlaRules\" WHERE \"Id\" = '6e8d8f7e-9f7c-4a8e-8b06-6d66c5d8e045';");
            migrationBuilder.Sql("UPDATE \"LeadSlaRules\" SET \"FirstResponseMinutes\" = (\"FirstResponseMinutes\" + 59) / 60;");
            migrationBuilder.RenameColumn(
                name: "FirstResponseMinutes",
                table: "LeadSlaRules",
                newName: "FirstResponseHours");
        }
    }
}
