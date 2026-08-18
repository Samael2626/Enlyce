using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class InteraccionTests
{
    private static Email ValidEmail() => Email.Create("test@example.com");

    [Fact]
    public void Registrar_Valido_ReturnsInteraccion()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        var asesorId = Guid.NewGuid();
        var interaccion = Interaccion.Registrar(lead.Id, asesorId, "Llamada", "Primer contacto");

        Assert.Equal(lead.Id, interaccion.LeadId);
        Assert.Equal(asesorId, interaccion.AsesorId);
        Assert.Equal("Llamada", interaccion.Tipo);
        Assert.Equal("Primer contacto", interaccion.Resumen);
        Assert.True(interaccion.Fecha <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Registrar_TipoVacio_ThrowsDomainError(string tipo)
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Throws<DomainError>(() =>
            Interaccion.Registrar(lead.Id, Guid.NewGuid(), tipo));
    }

    [Fact]
    public void Registrar_LeadVacio_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Interaccion.Registrar(Guid.Empty, Guid.NewGuid(), "Llamada"));
    }

    [Fact]
    public void Registrar_AsesorVacio_ThrowsDomainError()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Throws<DomainError>(() =>
            Interaccion.Registrar(lead.Id, Guid.Empty, "Llamada"));
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var asesorId = Guid.NewGuid();
        var fecha = DateTime.UtcNow;

        var interaccion = Interaccion.Reconstituir(id, leadId, asesorId, "Email", null, fecha);

        Assert.Equal(id, interaccion.Id);
        Assert.Equal(leadId, interaccion.LeadId);
        Assert.Equal(asesorId, interaccion.AsesorId);
        Assert.Equal(fecha, interaccion.Fecha);
    }
}
