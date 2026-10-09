using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackLeadStageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeadStageHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStage = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    NewStage = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadStageHistory_Asesores_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadStageHistory_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistory_ActorId",
                table: "LeadStageHistory",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistory_LeadId_ChangedAt",
                table: "LeadStageHistory",
                columns: new[] { "LeadId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadStageHistory");
        }
    }
}
