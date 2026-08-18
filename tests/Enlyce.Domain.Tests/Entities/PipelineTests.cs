using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class PipelineTests
{
    private static Email ValidEmail() => Email.Create("test@example.com");

    [Fact]
    public void MoverEtapa_TransicionValida_CambiaEtapa()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        lead.MoverEtapa(EtapasPipeline.Contactado);
        Assert.Equal(EtapasPipeline.Contactado, lead.EtapaPipeline);
    }

    [Fact]
    public void MoverEtapa_TransicionInvalida_ThrowsDomainError()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Throws<DomainError>(() =>
            lead.MoverEtapa(EtapasPipeline.Cualificado));
    }

    [Fact]
    public void MoverEtapa_Venta_FlujoCompleto()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);

        lead.MoverEtapa(EtapasPipeline.Contactado);
        lead.MoverEtapa(EtapasPipeline.Cualificado);
        lead.IncrementarInteracciones();
        lead.MoverEtapa(EtapasPipeline.VisitaAgendada);
        lead.MoverEtapa(EtapasPipeline.VisitaRealizada);
        lead.MoverEtapa(EtapasPipeline.OfertaNegociacion);
        lead.MoverEtapa(EtapasPipeline.BajoContrato);
        lead.MoverEtapa(EtapasPipeline.CerradoGanado);

        Assert.Equal(EtapasPipeline.CerradoGanado, lead.EtapaPipeline);
        Assert.Equal(EstadoLead.CerradoGanado, lead.Estado);
    }

    [Fact]
    public void MoverEtapa_Venta_CerrarPerdido()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        lead.MoverEtapa(EtapasPipeline.Contactado);
        lead.MoverEtapa(EtapasPipeline.CerradoPerdido);

        Assert.Equal(EtapasPipeline.CerradoPerdido, lead.EtapaPipeline);
        Assert.Equal(EstadoLead.CerradoPerdido, lead.Estado);
    }

    [Fact]
    public void MoverEtapa_Arriendo_FlujoCompleto()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true, "Arriendo");

        lead.MoverEtapa(EtapasPipeline.Contactado);
        lead.MoverEtapa(EtapasPipeline.Cualificado);
        lead.IncrementarInteracciones();
        lead.MoverEtapa(EtapasPipeline.VisitaAgendada);
        lead.MoverEtapa(EtapasPipeline.VisitaRealizada);
        lead.MoverEtapa(EtapasPipeline.Postulacion);
        lead.MoverEtapa(EtapasPipeline.AprobacionPropietario);
        lead.MoverEtapa(EtapasPipeline.CerradoGanado);

        Assert.Equal(EtapasPipeline.CerradoGanado, lead.EtapaPipeline);
    }

    [Fact]
    public void MoverEtapa_RequiereInteraccionParaVisita()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        lead.MoverEtapa(EtapasPipeline.Contactado);
        lead.MoverEtapa(EtapasPipeline.Cualificado);

        Assert.Throws<DomainError>(() =>
            lead.MoverEtapa(EtapasPipeline.VisitaAgendada));
    }

    [Fact]
    public void MoverEtapa_ConInteraccionesPermiteVisita()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        lead.MoverEtapa(EtapasPipeline.Contactado);
        lead.MoverEtapa(EtapasPipeline.Cualificado);
        lead.IncrementarInteracciones();

        lead.MoverEtapa(EtapasPipeline.VisitaAgendada);
        Assert.Equal(EtapasPipeline.VisitaAgendada, lead.EtapaPipeline);
    }

    [Fact]
    public void TipoOperacion_Venta_Creacion()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true, "Venta");
        Assert.Equal("Venta", lead.TipoOperacion);
        Assert.Equal(EtapasPipeline.LeadNuevo, lead.EtapaPipeline);
    }

    [Fact]
    public void TipoOperacion_Arriendo_Creacion()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true, "Arriendo");
        Assert.Equal("Arriendo", lead.TipoOperacion);
    }

    [Fact]
    public void TipoOperacion_Invalido_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() =>
            Lead.Crear("Juan", ValidEmail(), null, "Web", true, "Alquiler"));
    }

    [Fact]
    public void IncrementarInteracciones_ActualizaContador()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Equal(0, lead.InteraccionesCount);

        lead.IncrementarInteracciones();
        Assert.Equal(1, lead.InteraccionesCount);
        Assert.NotNull(lead.FechaUltimaInteraccion);

        lead.IncrementarInteracciones();
        Assert.Equal(2, lead.InteraccionesCount);
    }

    [Fact]
    public void Asignar_CambiaEtapaAContactado()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "Web", true);
        Assert.Equal(EtapasPipeline.LeadNuevo, lead.EtapaPipeline);

        lead.AsignarAsesor(Guid.NewGuid());
        Assert.Equal(EstadoLead.Contactado, lead.Estado);
        Assert.NotNull(lead.AsesorAsignadoId);
    }

    [Fact]
    public void EtapasPipeline_Transiciones_Venta()
    {
        var siguientes = TransicionesPipeline.ObtenerSiguientes(
            EtapasPipeline.LeadNuevo, "Venta");
        Assert.Contains(EtapasPipeline.Contactado, siguientes);
        Assert.Contains(EtapasPipeline.CerradoPerdido, siguientes);
    }

    [Fact]
    public void EtapasPipeline_Transiciones_Arriendo()
    {
        var siguientes = TransicionesPipeline.ObtenerSiguientes(
            EtapasPipeline.VisitaRealizada, "Arriendo");
        Assert.Contains(EtapasPipeline.Postulacion, siguientes);
        Assert.DoesNotContain(EtapasPipeline.OfertaNegociacion, siguientes);
    }
}
