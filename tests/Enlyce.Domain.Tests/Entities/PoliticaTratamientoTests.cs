using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public class PoliticaTratamientoTests
{
    [Fact]
    public void Crear_ConDatosValidos_CreaPolitica()
    {
        var fecha = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var politica = PoliticaTratamiento.Crear("1.0", "Texto de politica", fecha);

        Assert.Equal("1.0", politica.Version);
        Assert.Equal("Texto de politica", politica.TextoCompleto);
        Assert.Equal(fecha, politica.FechaVigencia);
        Assert.True(politica.Activa);
    }

    [Fact]
    public void Crear_VersionVacia_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            PoliticaTratamiento.Crear("", "Texto", DateTime.UtcNow));
    }

    [Fact]
    public void Crear_TextoVacio_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            PoliticaTratamiento.Crear("1.0", "", DateTime.UtcNow));
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var politica = PoliticaTratamiento.Crear("1.0", "Texto", DateTime.UtcNow);
        politica.Desactivar();
        Assert.False(politica.Activa);
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var fecha = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var politica = PoliticaTratamiento.Reconstituir(id, "2.0", "Texto largo", fecha, false);

        Assert.Equal(id, politica.Id);
        Assert.Equal("2.0", politica.Version);
        Assert.Equal("Texto largo", politica.TextoCompleto);
        Assert.Equal(fecha, politica.FechaVigencia);
        Assert.False(politica.Activa);
    }
}
