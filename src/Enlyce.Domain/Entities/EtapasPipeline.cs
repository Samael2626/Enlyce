namespace Enlyce.Domain.Entities;

public static class EtapasPipeline
{
    public const string LeadNuevo = "Lead nuevo";
    public const string Contactado = "Contactado";
    public const string Cualificado = "Cualificado";
    public const string VisitaAgendada = "Visita agendada";
    public const string VisitaRealizada = "Visita realizada";
    public const string OfertaNegociacion = "Oferta/Negociacion";
    public const string BajoContrato = "Bajo contrato";
    public const string CerradoGanado = "Cerrado ganado";
    public const string CerradoPerdido = "Cerrado perdido";

    public const string Postulacion = "Postulacion";
    public const string AprobacionPropietario = "Aprobacion propietario";

    public static readonly string[] Venta =
    [
        LeadNuevo, Contactado, Cualificado, VisitaAgendada,
        VisitaRealizada, OfertaNegociacion, BajoContrato,
        CerradoGanado, CerradoPerdido
    ];

    public static readonly string[] Arriendo =
    [
        LeadNuevo, Contactado, Cualificado, VisitaAgendada,
        VisitaRealizada, Postulacion, AprobacionPropietario,
        CerradoGanado, CerradoPerdido
    ];

    public static readonly Dictionary<string, string> Etiquetas = new()
    {
        [LeadNuevo] = "Lead nuevo",
        [Contactado] = "Contactado",
        [Cualificado] = "Cualificado",
        [VisitaAgendada] = "Visita agendada",
        [VisitaRealizada] = "Visita realizada",
        [OfertaNegociacion] = "Oferta / Negociacion",
        [BajoContrato] = "Bajo contrato",
        [CerradoGanado] = "Cerrado ganado",
        [CerradoPerdido] = "Cerrado perdido",
        [Postulacion] = "Postulacion",
        [AprobacionPropietario] = "Aprobacion propietario"
    };

    public static bool EsValida(string etapa, string tipoOperacion)
    {
        var etapas = tipoOperacion == "Arriendo" ? Arriendo : Venta;
        return etapas.Contains(etapa);
    }

    public static bool EsTipoOperacionValida(string tipoOperacion)
    {
        return tipoOperacion == "Venta" || tipoOperacion == "Arriendo";
    }
}
