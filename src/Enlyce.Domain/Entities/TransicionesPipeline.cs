namespace Enlyce.Domain.Entities;

public static class TransicionesPipeline
{
    private static readonly Dictionary<string, string[]> Venta = new()
    {
        [EtapasPipeline.LeadNuevo] = [EtapasPipeline.Contactado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Contactado] = [EtapasPipeline.Cualificado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Cualificado] = [EtapasPipeline.VisitaAgendada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaAgendada] = [EtapasPipeline.VisitaRealizada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaRealizada] = [EtapasPipeline.OfertaNegociacion, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.OfertaNegociacion] = [EtapasPipeline.BajoContrato, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.BajoContrato] = [EtapasPipeline.CerradoGanado, EtapasPipeline.CerradoPerdido],
    };

    private static readonly Dictionary<string, string[]> Arriendo = new()
    {
        [EtapasPipeline.LeadNuevo] = [EtapasPipeline.Contactado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Contactado] = [EtapasPipeline.Cualificado, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Cualificado] = [EtapasPipeline.VisitaAgendada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaAgendada] = [EtapasPipeline.VisitaRealizada, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.VisitaRealizada] = [EtapasPipeline.Postulacion, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.Postulacion] = [EtapasPipeline.AprobacionPropietario, EtapasPipeline.CerradoPerdido],
        [EtapasPipeline.AprobacionPropietario] = [EtapasPipeline.CerradoGanado, EtapasPipeline.CerradoPerdido],
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
