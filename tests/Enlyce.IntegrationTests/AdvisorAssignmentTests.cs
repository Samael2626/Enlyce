using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class AdvisorAssignmentTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task LeastBusyAssignment_ChoosesActiveAdvisorWithLowestOpenWorkload()
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var busy = CreateAdvisor("busy", "Asesor");
        var available = CreateAdvisor("available", "Asesor");
        var admin = CreateAdvisor("admin", "Administrador");
        var inactive = CreateAdvisor("inactive", "Asesor", active: false);
        db.Asesores.AddRange(busy, available, admin, inactive);
        var openLead = Lead.Crear("Abierto", Email.Create($"open-{Guid.NewGuid():N}@test.com"), null,
            "Web", true);
        openLead.AsignarAsesor(busy.Id);
        db.Leads.Add(openLead);
        var closedLead = Lead.Crear("Cerrado", Email.Create($"closed-{Guid.NewGuid():N}@test.com"), null,
            "Web", true);
        closedLead.AsignarAsesor(available.Id);
        closedLead.MoverEtapa(EtapasPipeline.CerradoPerdido);
        db.Leads.Add(closedLead);
        await db.SaveChangesAsync();

        var assignment = await scope.ServiceProvider.GetRequiredService<IAsesorRepository>()
            .ObtenerAsesorConMenosOportunidadesAbiertasAsync();

        Assert.Equal(available.Id, assignment);
    }

    [Fact]
    public async Task LeastBusyAssignment_ReturnsNullWithoutActiveAdvisors()
    {
        using var isolatedFactory = new TestWebApplicationFactory();
        using var client = isolatedFactory.CreateClient();
        using var scope = isolatedFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var inactive = CreateAdvisor("only-inactive", "Asesor", active: false);
        var admin = CreateAdvisor("only-admin", "Administrador");
        db.Asesores.AddRange(inactive, admin);
        await db.SaveChangesAsync();

        var assignment = await scope.ServiceProvider.GetRequiredService<IAsesorRepository>()
            .ObtenerAsesorConMenosOportunidadesAbiertasAsync();

        Assert.Null(assignment);
    }

    [Fact]
    public async Task RoundRobinAssignment_DistributesConsecutiveLeadsAcrossActiveAdvisors()
    {
        using var isolatedFactory = new TestWebApplicationFactory();
        using var client = isolatedFactory.CreateClient();
        using var scope = isolatedFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var advisors = Enumerable.Range(0, 3)
            .Select(index => CreateAdvisor($"rotation-{index}", "Asesor"))
            .ToArray();
        db.Asesores.AddRange(advisors);
        await db.SaveChangesAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IAsesorRepository>();
        var assignments = new List<Guid?>();
        for (var index = 0; index < advisors.Length; index++)
            assignments.Add(await repository.ObtenerSiguienteAsesorEnRotacionAsync());

        Assert.Equal(advisors.Select(advisor => advisor.Id).Order().ToArray(),
            assignments.Select(id => id!.Value).Order().ToArray());
    }

    private static Asesor CreateAdvisor(string prefix, string role, bool active = true) =>
        Asesor.Reconstituir(Guid.NewGuid(), prefix, Email.Create($"{prefix}-{Guid.NewGuid():N}@test.com"),
            "test-hash", role, DateTime.UtcNow, active);
}
