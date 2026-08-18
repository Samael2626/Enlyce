using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class VisitaTests
{
    private static Email ValidEmail() => Email.Create("test@example.com");

    [Fact]
    public void Programar_Valido_ReturnsVisita()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        var inmuebleId = Guid.NewGuid();
        var asesorId = Guid.NewGuid();
        var fecha = DateTime.UtcNow.AddDays(1);

        var visita = Visita.Programar(lead.Id, inmuebleId, asesorId, fecha);

        Assert.Equal(lead.Id, visita.LeadId);
        Assert.Equal(inmuebleId, visita.InmuebleId);
        Assert.Equal(asesorId, visita.AsesorId);
        Assert.Equal(fecha, visita.FechaProgramada);
        Assert.Equal("Programada", visita.Estado);
    }

    [Fact]
    public void Programar_LeadVacio_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Visita.Programar(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Programar_InmuebleVacio_ThrowsDomainError()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Throws<DomainError>(() =>
            Visita.Programar(lead.Id, Guid.Empty, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Programar_AsesorVacio_ThrowsDomainError()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Throws<DomainError>(() =>
            Visita.Programar(lead.Id, Guid.NewGuid(), Guid.Empty, DateTime.UtcNow));
    }

    [Fact]
    public void MarcarRealizada_CambiaEstado()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        var visita = Visita.Programar(lead.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        visita.MarcarRealizada("Muy interesado");

        Assert.Equal("Realizada", visita.Estado);
        Assert.NotNull(visita.FechaRealizada);
        Assert.Equal("Muy interesado", visita.Feedback);
    }

    [Fact]
    public void Cancelar_CambiaEstado()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        var visita = Visita.Programar(lead.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        visita.Cancelar();

        Assert.Equal("Cancelada", visita.Estado);
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var fecha = DateTime.UtcNow;
        var visita = Visita.Reconstituir(
            id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            fecha, fecha.AddHours(1), "Excelente", "Realizada");

        Assert.Equal(id, visita.Id);
        Assert.Equal("Realizada", visita.Estado);
        Assert.Equal("Excelente", visita.Feedback);
    }
}
