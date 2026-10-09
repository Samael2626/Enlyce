using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackLeadAssignmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeadAssignmentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousAdvisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewAdvisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedByAdvisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadAssignmentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadAssignmentHistory_Asesores_ChangedByAdvisorId",
                        column: x => x.ChangedByAdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadAssignmentHistory_Asesores_NewAdvisorId",
                        column: x => x.NewAdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadAssignmentHistory_Asesores_PreviousAdvisorId",
                        column: x => x.PreviousAdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadAssignmentHistory_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignmentHistory_ChangedByAdvisorId",
                table: "LeadAssignmentHistory",
                column: "ChangedByAdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignmentHistory_LeadId_ChangedAt",
                table: "LeadAssignmentHistory",
                columns: new[] { "LeadId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignmentHistory_NewAdvisorId",
                table: "LeadAssignmentHistory",
                column: "NewAdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignmentHistory_PreviousAdvisorId",
                table: "LeadAssignmentHistory",
                column: "PreviousAdvisorId");

            migrationBuilder.Sql("""
                INSERT INTO "LeadAssignmentHistory"
                    ("Id", "LeadId", "PreviousAdvisorId", "NewAdvisorId", "ChangedByAdvisorId", "Reason", "Source", "ChangedAt")
                SELECT gen_random_uuid(), "Id", NULL, "AsesorAsignadoId", NULL,
                       'Asignacion existente previa al historial', 'Legacy',
                       COALESCE("FechaAsignacion", "FechaCreacion")
                FROM "Leads"
                WHERE "AsesorAsignadoId" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadAssignmentHistory");
        }
    }
}
