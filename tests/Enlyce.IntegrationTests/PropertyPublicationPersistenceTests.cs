using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.IntegrationTests;

public sealed class PropertyPublicationPersistenceTests
{
    [Fact]
    public async Task SaveAndLoad_PublishedAggregate_RehydratesPhotosAndState()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<EnlyceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new EnlyceDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var advisor = Asesor.Crear(
            "Asesor Web",
            Email.Create("web@example.test"),
            "hash-for-persistence-test");
        var property = Inmueble.Crear(
            "Apartamento Laureles",
            "Inmueble de prueba",
            TipoInmueble.Apartamento,
            ModalidadInmueble.Venta,
            Direccion.Crear("Dirección privada", "Medellín", "Laureles"),
            Dinero.Crear(620_000_000m),
            92,
            3,
            2,
            1,
            Guid.NewGuid());
        var publication = PropertyPublication.Create(
            property.Id,
            advisor.Id,
            "Apartamento en Laureles",
            "Apartamento iluminado",
            "Cerca de servicios y vías principales.");
        publication.SetPublicPrice(620_000_000m);
        publication.SetPublicLocation("Medellín", "Laureles", 6.2443m, -75.5934m);
        publication.AddPhoto("https://media.example.test/1.webp", "Sala", 0, true);
        publication.AddPhoto("https://media.example.test/2.webp", "Cocina", 1);
        publication.AddPhoto("https://media.example.test/3.webp", "Habitación", 2);
        publication.Publish();

        db.Asesores.Add(advisor);
        db.Inmuebles.Add(property);
        db.PropertyPublications.Add(publication);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var restored = await db.PropertyPublications
            .Include(item => item.Photos)
            .SingleAsync(item => item.Id == publication.Id);

        Assert.Equal(PublicationStatus.Published, restored.Status);
        Assert.Equal("apartamento-en-laureles", restored.Slug);
        Assert.Equal(3, restored.Photos.Count);
        Assert.Single(restored.Photos, photo => photo.IsCover);
        Assert.False(restored.ExactAddressVisible);
    }
}
