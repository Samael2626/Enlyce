using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadsPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EtapaPipeline",
                table: "Leads",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaInteraccion",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InteraccionesCount",
                table: "Leads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoOperacion",
                table: "Leads",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Interacciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsesorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Resumen = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interacciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interacciones_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    InmuebleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsesorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRealizada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AsesorAsignadoId",
                table: "Leads",
                column: "AsesorAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_EtapaPipeline",
                table: "Leads",
                column: "EtapaPipeline");

            migrationBuilder.CreateIndex(
                name: "IX_Interacciones_LeadId",
                table: "Interacciones",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_AsesorId",
                table: "Visitas",
                column: "AsesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_LeadId",
                table: "Visitas",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interacciones");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Leads_AsesorAsignadoId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_EtapaPipeline",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EtapaPipeline",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FechaUltimaInteraccion",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "InteraccionesCount",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TipoOperacion",
                table: "Leads");
        }
    }
}
