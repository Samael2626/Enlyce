using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedondearCoordenadasPublicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El redondeo en SetPublicLocation solo cubre escrituras nuevas. Las
            // filas sembradas antes conservan hasta seis decimales y EF las
            // materializa tal cual, asi que el catalogo publico seguiria
            // exponiendolas. Esto limpia lo ya guardado.
            migrationBuilder.Sql("""
                UPDATE "PropertyPublications"
                SET "ApproximateLatitude" = ROUND("ApproximateLatitude", 3),
                    "ApproximateLongitude" = ROUND("ApproximateLongitude", 3)
                WHERE "ApproximateLatitude" IS NOT NULL
                   OR "ApproximateLongitude" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sin vuelta atras a proposito: la precision descartada es
            // justamente el dato que no debia ser publico. Revertir exigiria
            // recargar las coordenadas desde la fuente original.
        }
    }
}
