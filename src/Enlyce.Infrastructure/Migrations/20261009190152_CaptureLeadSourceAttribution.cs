using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaptureLeadSourceAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceRoute",
                table: "Leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "Leads",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceRoute",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "Leads");
        }
    }
}
