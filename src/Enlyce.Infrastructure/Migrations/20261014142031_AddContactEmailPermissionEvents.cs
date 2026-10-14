using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactEmailPermissionEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION reject_contact_email_permission_event_mutation() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'Contact email permission events are append-only';
                END;
                $$ LANGUAGE plpgsql;
                """);
            migrationBuilder.CreateTable(
                name: "ContactEmailPermissionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    Allowed = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactEmailPermissionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactEmailPermissionEvents_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmailPermissionEvents_ContactId_OccurredAt",
                table: "ContactEmailPermissionEvents",
                columns: new[] { "ContactId", "OccurredAt" });

            migrationBuilder.Sql("""
                CREATE TRIGGER TR_ContactEmailPermissionEvents_AppendOnly
                BEFORE UPDATE OR DELETE ON "ContactEmailPermissionEvents"
                FOR EACH ROW EXECUTE FUNCTION reject_contact_email_permission_event_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactEmailPermissionEvents");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS reject_contact_email_permission_event_mutation();");
        }
    }
}
