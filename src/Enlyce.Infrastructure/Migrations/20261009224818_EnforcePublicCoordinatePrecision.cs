using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePublicCoordinatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyPublications_ApproximateLatitude_ThreeDecimals",
                table: "PropertyPublications",
                sql: "\"ApproximateLatitude\" IS NULL OR ROUND(\"ApproximateLatitude\", 3) = \"ApproximateLatitude\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyPublications_ApproximateLongitude_ThreeDecimals",
                table: "PropertyPublications",
                sql: "\"ApproximateLongitude\" IS NULL OR ROUND(\"ApproximateLongitude\", 3) = \"ApproximateLongitude\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyPublications_ApproximateLatitude_ThreeDecimals",
                table: "PropertyPublications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyPublications_ApproximateLongitude_ThreeDecimals",
                table: "PropertyPublications");
        }
    }
}
