using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Enlyce.IntegrationTests;

public sealed class PublicCoordinatePostgresConstraintTests
{
    [PostgresFact]
    public async Task Postgres_AllowsThreeDecimalsAndNulls_ButRejectsSix()
    {
        var configuredConnection = Environment.GetEnvironmentVariable("ENLYCE_TEST_POSTGRES_CONNECTION");
        var adminBuilder = new NpgsqlConnectionStringBuilder(configuredConnection!);
        var host = adminBuilder.Host;
        var database = adminBuilder.Database;
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) ||
            !new[] { "localhost", "127.0.0.1", "::1" }.Contains(host, StringComparer.OrdinalIgnoreCase) ||
            !database.Contains("test", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La prueba exige una BD PostgreSQL local desechable cuyo nombre incluya 'test'.");

        var schema = $"coordinate_check_{Guid.NewGuid():N}";
        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync();
        await using (var createSchema = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", adminConnection))
            await createSchema.ExecuteNonQueryAsync();

        try
        {
            var testBuilder = new NpgsqlConnectionStringBuilder(adminBuilder.ConnectionString)
            {
                SearchPath = schema,
                Pooling = false,
            };
            var options = new DbContextOptionsBuilder<EnlyceDbContext>()
                .UseNpgsql(testBuilder.ConnectionString)
                .Options;

            await using var db = new EnlyceDbContext(options);
            await db.Database.MigrateAsync("20261010010224_TrackPaymentOrderStatusChanges");
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "LeadSlaRules"
                    ("Id", "SourceKey", "OperationType", "FirstResponseHours", "InactivityDays", "Enabled", "UpdatedAtUtc")
                VALUES ('4a8fd335-dc3d-44b6-8df2-8ad5d9df9b19', 'PORTAL', 'Venta', 2, NULL, TRUE, CURRENT_TIMESTAMP)
                """);
            var migrationScript = db.GetService<IMigrator>().GenerateScript(
                "20261010010224_TrackPaymentOrderStatusChanges",
                options: MigrationsSqlGenerationOptions.Idempotent);
            await db.Database.ExecuteSqlRawAsync(migrationScript);

            var slaRules = await db.LeadSlaRules.ToListAsync();
            Assert.Equal(2, slaRules.Count);
            Assert.Equal(45, Assert.Single(slaRules, rule => rule.SourceKey == "PORTAL").FirstResponseMinutes);
            var defaultSla = Assert.Single(slaRules, rule => rule.SourceKey == "*" && rule.OperationType == "*");
            Assert.Equal(45, defaultSla.FirstResponseMinutes);
            Assert.True(defaultSla.Enabled);

            var owner = Propietario.Crear(
                "Dueño de prueba",
                Email.Create($"owner-{Guid.NewGuid():N}@example.test"));
            var advisor = Asesor.Crear(
                "Asesor de prueba",
                Email.Create($"advisor-{Guid.NewGuid():N}@example.test"),
                "test-hash");
            var property = Inmueble.Crear(
                "Inmueble de prueba",
                "Descripción de prueba",
                TipoInmueble.Apartamento,
                ModalidadInmueble.Venta,
                Direccion.Crear("Calle privada", "Medellín", "Laureles"),
                Dinero.Crear(100_000_000m),
                50,
                2,
                1,
                1,
                owner.Id);
            var publication = PropertyPublication.Create(
                property.Id,
                advisor.Id,
                "apartamento-prueba",
                "Apartamento de prueba",
                "Descripción pública de prueba");
            publication.SetPublicPrice(100_000_000m);
            publication.SetPublicLocation("Medellín", "Laureles", 6.244m, -75.593m);

            db.Propietarios.Add(owner);
            db.Asesores.Add(advisor);
            db.Inmuebles.Add(property);
            db.PropertyPublications.Add(publication);
            await db.SaveChangesAsync();

            var coordinates = await db.PropertyPublications
                .Where(item => item.Id == publication.Id)
                .Select(item => new { item.ApproximateLatitude, item.ApproximateLongitude })
                .SingleAsync();
            Assert.Equal(6.244m, coordinates.ApproximateLatitude);
            Assert.Equal(-75.593m, coordinates.ApproximateLongitude);

            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"PropertyPublications\" SET \"ApproximateLatitude\" = NULL, \"ApproximateLongitude\" = NULL WHERE \"Id\" = {publication.Id}");

            var latitudeViolation = await Assert.ThrowsAsync<PostgresException>(() =>
                db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"PropertyPublications\" SET \"ApproximateLatitude\" = 6.244321 WHERE \"Id\" = {publication.Id}"));
            Assert.Equal(PostgresErrorCodes.CheckViolation, latitudeViolation.SqlState);

            var longitudeViolation = await Assert.ThrowsAsync<PostgresException>(() =>
                db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"PropertyPublications\" SET \"ApproximateLongitude\" = -75.593321 WHERE \"Id\" = {publication.Id}"));
            Assert.Equal(PostgresErrorCodes.CheckViolation, longitudeViolation.SqlState);
        }
        finally
        {
            await using var dropSchema = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", adminConnection);
            await dropSchema.ExecuteNonQueryAsync();
        }
    }
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ENLYCE_TEST_POSTGRES_CONNECTION")))
            Skip = "Define ENLYCE_TEST_POSTGRES_CONNECTION para correr contra PostgreSQL desechable.";
    }
}
