using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public class ConsentimientoTests
{
    [Fact]
    public void Registrar_ConDatosValidos_CreaConsentimiento()
    {
        var leadId = Guid.NewGuid();
        var consentimiento = Consentimiento.Registrar(
            leadId, "Acepto tratamiento de datos", "1.0", "checkbox_web", "127.0.0.1");

        Assert.Equal(leadId, consentimiento.LeadId);
        Assert.Equal("Acepto tratamiento de datos", consentimiento.TextoConsentido);
        Assert.Equal("1.0", consentimiento.VersionPolitica);
        Assert.Equal("checkbox_web", consentimiento.Metodo);
        Assert.Equal("127.0.0.1", consentimiento.DireccionIp);
        Assert.True(consentimiento.Fecha <= DateTime.UtcNow);
    }

    [Fact]
    public void Registrar_SinDireccionIp_Funciona()
    {
        var consentimiento = Consentimiento.Registrar(
            Guid.NewGuid(), "Acepto", "1.0", "whatsapp");

        Assert.Null(consentimiento.DireccionIp);
    }

    [Fact]
    public void Registrar_LeadIdVacio_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Consentimiento.Registrar(Guid.Empty, "Acepto", "1.0", "checkbox_web"));
    }

    [Fact]
    public void Registrar_TextoConsentidoVacio_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Consentimiento.Registrar(Guid.NewGuid(), "", "1.0", "checkbox_web"));
    }

    [Fact]
    public void Registrar_VersionPoliticaVacia_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Consentimiento.Registrar(Guid.NewGuid(), "Acepto", "", "checkbox_web"));
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var fecha = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var consentimiento = Consentimiento.Reconstituir(
            id, leadId, fecha, "192.168.1.1", "Texto", "1.0", "presencial");

        Assert.Equal(id, consentimiento.Id);
        Assert.Equal(leadId, consentimiento.LeadId);
        Assert.Equal(fecha, consentimiento.Fecha);
        Assert.Equal("192.168.1.1", consentimiento.DireccionIp);
        Assert.Equal("Texto", consentimiento.TextoConsentido);
        Assert.Equal("1.0", consentimiento.VersionPolitica);
        Assert.Equal("presencial", consentimiento.Metodo);
    }
}
