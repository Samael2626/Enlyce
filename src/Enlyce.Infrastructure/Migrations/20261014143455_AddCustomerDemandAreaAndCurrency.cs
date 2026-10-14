using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDemandAreaAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CustomerDemands",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP");

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumArea",
                table: "CustomerDemands",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumArea",
                table: "CustomerDemands",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CustomerDemands");

            migrationBuilder.DropColumn(
                name: "MaximumArea",
                table: "CustomerDemands");

            migrationBuilder.DropColumn(
                name: "MinimumArea",
                table: "CustomerDemands");
        }
    }
}
