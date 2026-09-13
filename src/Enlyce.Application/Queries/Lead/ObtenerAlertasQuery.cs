namespace Enlyce.Application.Queries.Lead;

public sealed record ObtenerAlertasQuery(int DiasUmbral = 7, Guid? AdvisorId = null);

public sealed record AlertaLeadDto(
    Guid Id, string Nombre, string Email, string EtapaPipeline,
    DateTime FechaUltimaActividad, int DiasSinActividad);

public sealed record AlertasResponse(List<AlertaLeadDto> Leads, int Total);
