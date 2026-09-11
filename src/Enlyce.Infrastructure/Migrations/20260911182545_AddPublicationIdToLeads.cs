using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicationIdToLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicationId",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PublicationId",
                table: "Leads",
                column: "PublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_PublicationId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PublicationId",
                table: "Leads");
        }
    }
}
