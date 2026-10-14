using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCountryDepartmentAndDemandLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Inmuebles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Inmuebles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Colombia");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CustomerDemands",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Colombia");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "CustomerDemands",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Inmuebles");

            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Inmuebles");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "CustomerDemands");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "CustomerDemands");
        }
    }
}
