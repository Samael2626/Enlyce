using Enlyce.Api.Development;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using NSubstitute;
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

        var storage = Substitute.For<IMediaStorage>();
        storage.StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var upload = call.Arg<MediaUpload>();
                var url = $"https://cdn.enlyce.test/media/publicaciones/{upload.PublicationId}/{upload.FileName}.webp";
                return Task.FromResult(new StoredMedia(url, 1600, 1067, [new MediaVariant(url, 1600)]));
            });

        var photoSource = Path.Combine(AppContext.BaseDirectory, "demo-photos");
        Directory.CreateDirectory(photoSource);
        foreach (var name in new[] { "property-triptych.png", "medellin-hero.png" })
            await File.WriteAllBytesAsync(Path.Combine(photoSource, name), [1, 2, 3, 4]);

        await DemoCatalogSeeder.SeedAsync(db, storage, photoSource);
        await DemoCatalogSeeder.SeedAsync(db, storage, photoSource);

        Assert.Equal(12, await db.PropertyPublications.CountAsync());
        Assert.Equal(12, await db.PropertyPublications.CountAsync(item => item.Status == PublicationStatus.Published));
        Assert.Equal(12, await db.Inmuebles.CountAsync());
        Assert.Equal(36, await db.PropertyPhotos.CountAsync());
        Assert.Single(await db.Propietarios.ToListAsync());
        Assert.Single(await db.Asesores.ToListAsync());
    }
}
