using Enlyce.Api.Development;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.IntegrationTests;

public sealed class DemoCatalogSeederTests
{
    [Fact]
    public async Task SeedAsync_CalledTwice_CreatesTwelvePublishedPropertiesWithoutDuplicates()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<EnlyceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new EnlyceDbContext(options);
        await db.Database.EnsureCreatedAsync();

        await DemoCatalogSeeder.SeedAsync(db);
        await DemoCatalogSeeder.SeedAsync(db);

        Assert.Equal(12, await db.PropertyPublications.CountAsync());
        Assert.Equal(12, await db.PropertyPublications.CountAsync(item => item.Status == PublicationStatus.Published));
        Assert.Equal(12, await db.Inmuebles.CountAsync());
        Assert.Equal(36, await db.PropertyPhotos.CountAsync());
        Assert.Single(await db.Propietarios.ToListAsync());
        Assert.Single(await db.Asesores.ToListAsync());
    }
}
