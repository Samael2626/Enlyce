using Enlyce.Application.Auth;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enlyce.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    private bool _schemaCreated;

    public static Guid PropietarioSeedId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public TestWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<EnlyceDbContext>) ||
                d.ServiceType == typeof(EnlyceDbContext) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType != null && d.ServiceType.FullName != null &&
                 d.ServiceType.FullName.Contains("DbContextOptions")) ||
                (d.ImplementationType != null && d.ImplementationType.FullName != null &&
                 d.ImplementationType.FullName.Contains("DbContext"))).ToList();

            foreach (var d in descriptors)
                services.Remove(d);

            services.AddDbContext<EnlyceDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddSingleton(Options.Create(new JwtSettings
            {
                SecretKey = "TestSecretKeyForTests123456789012345",
                Issuer = "EnlyceTest",
                Audience = "EnlyceTest",
                ExpirationMinutes = 60
            }));
        });

        var host = base.CreateHost(builder);

        if (!_schemaCreated)
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Database.EnsureCreated();
            SeedPropietario(db);
            SeedPolitica(db);
            _schemaCreated = true;
        }

        return host;
    }

    private static void SeedPropietario(EnlyceDbContext db)
    {
        if (db.Propietarios.Any(p => p.Id == PropietarioSeedId))
            return;

        var propietario = Propietario.Reconstituir(
            PropietarioSeedId,
            "Propietario Test",
            Email.Create("propietario@test.com"),
            Telefono.Create("3100000000"),
            DateTime.UtcNow,
            true);

        db.Propietarios.Add(propietario);
        db.SaveChanges();
    }

    private static void SeedPolitica(EnlyceDbContext db)
    {
        if (db.PoliticasTratamiento.Any())
            return;

        var politica = PoliticaTratamiento.Crear(
            "1.0",
            "Politica de tratamiento de datos personales conforme a la Ley 1581 de 2012.",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        db.PoliticasTratamiento.Add(politica);
        db.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
