using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAsesorResponsableInmueble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsesorResponsableId",
                table: "Inmuebles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inmuebles_AsesorResponsableId",
                table: "Inmuebles",
                column: "AsesorResponsableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inmuebles_Asesores_AsesorResponsableId",
                table: "Inmuebles",
                column: "AsesorResponsableId",
                principalTable: "Asesores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inmuebles_Asesores_AsesorResponsableId",
                table: "Inmuebles");

            migrationBuilder.DropIndex(
                name: "IX_Inmuebles_AsesorResponsableId",
                table: "Inmuebles");

            migrationBuilder.DropColumn(
                name: "AsesorResponsableId",
                table: "Inmuebles");
        }
    }
}
