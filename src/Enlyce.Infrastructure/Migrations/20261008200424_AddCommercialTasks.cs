using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommercialTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdvisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReminderAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommercialTasks_Asesores_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialTasks_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialTasks_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommercialTaskEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialTaskEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommercialTaskEvents_Asesores_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialTaskEvents_CommercialTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CommercialTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialTaskEvents_ActorId",
                table: "CommercialTaskEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialTaskEvents_TaskId_OccurredAt",
                table: "CommercialTaskEvents",
                columns: new[] { "TaskId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialTasks_AdvisorId_DueAt",
                table: "CommercialTasks",
                columns: new[] { "AdvisorId", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialTasks_ContactId_DueAt",
                table: "CommercialTasks",
                columns: new[] { "ContactId", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialTasks_LeadId",
                table: "CommercialTasks",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommercialTaskEvents");

            migrationBuilder.DropTable(
                name: "CommercialTasks");
        }
    }
}
