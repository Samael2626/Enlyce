namespace Enlyce.Domain.Entities;

public static class TransicionesPipeline
{
    private static readonly Dictionary<string, string[]> Venta = new()
    {
        [EtapasPipeline.LeadNuevo] = [EtapasPipeline.Contactado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Contactado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Cualificado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Cualificado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.VisitaAgendada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaAgendada] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaRealizada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaRealizada] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.OfertaNegociacion, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.OfertaNegociacion] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.BajoContrato, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.BajoContrato] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.OfertaNegociacion, EtapasPipeline.CerradoGanado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.CerradoGanado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.OfertaNegociacion, EtapasPipeline.BajoContrato],
        [EtapasPipeline.CerradoPerdido] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.OfertaNegociacion, EtapasPipeline.BajoContrato, EtapasPipeline.CerradoGanado]
    };

    private static readonly Dictionary<string, string[]> Arriendo = new()
    {
        [EtapasPipeline.LeadNuevo] = [EtapasPipeline.Contactado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Contactado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Cualificado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Cualificado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.VisitaAgendada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaAgendada] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaRealizada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaRealizada] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.Postulacion, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Postulacion] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.AprobacionPropietario, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.AprobacionPropietario] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.Postulacion, EtapasPipeline.CerradoGanado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.CerradoGanado] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.Postulacion, EtapasPipeline.AprobacionPropietario],
        [EtapasPipeline.CerradoPerdido] = [EtapasPipeline.LeadNuevo, EtapasPipeline.Contactado, EtapasPipeline.Cualificado, EtapasPipeline.VisitaAgendada, EtapasPipeline.VisitaRealizada, EtapasPipeline.Postulacion, EtapasPipeline.AprobacionPropietario, EtapasPipeline.CerradoGanado]
    };

    public static bool EsTransicionValida(string etapaActual, string etapaNueva, string tipoOperacion)
    {
        var transiciones = tipoOperacion == "Arriendo" ? Arriendo : Venta;
        return transiciones.TryGetValue(etapaActual, out var siguientes) && siguientes.Contains(etapaNueva);
    }

    public static string[] ObtenerSiguientes(string etapaActual, string tipoOperacion)
    {
        var transiciones = tipoOperacion == "Arriendo" ? Arriendo : Venta;
        return transiciones.TryGetValue(etapaActual, out var siguientes) ? siguientes : [];
    }
}
