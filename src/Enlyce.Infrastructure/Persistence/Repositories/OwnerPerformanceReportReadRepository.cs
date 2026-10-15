using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class OwnerPerformanceReportReadRepository(EnlyceDbContext context)
    : IOwnerPerformanceReportReadRepository
{
    public async Task<OwnerPerformanceReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = through.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var owners = await context.Propietarios.AsNoTracking()
            .OrderBy(owner => owner.Nombre)
            .Select(owner => new { owner.Id, owner.Nombre })
            .ToArrayAsync(ct);

        var pageVisits = await (
            from analyticsEvent in context.WebAnalyticsEvents.AsNoTracking()
            join publication in context.PropertyPublications.AsNoTracking()
                on analyticsEvent.PropertySlug equals publication.Slug
            join property in context.Inmuebles.AsNoTracking()
                on publication.PropertyId equals property.Id
            where analyticsEvent.EventType == WebAnalyticsEventType.PageView &&
                  analyticsEvent.OccurredAt >= start && analyticsEvent.OccurredAt < end
            group analyticsEvent by property.PropietarioId into ownerEvents
            select new OwnerCount(ownerEvents.Key, ownerEvents.Count()))
            .ToArrayAsync(ct);

        var interactions = await (
            from interaction in context.Interacciones.AsNoTracking()
            join lead in context.Leads.AsNoTracking() on interaction.LeadId equals lead.Id
            join publication in context.PropertyPublications.AsNoTracking() on lead.PublicationId equals publication.Id
            join property in context.Inmuebles.AsNoTracking() on publication.PropertyId equals property.Id
            where interaction.Fecha >= start && interaction.Fecha < end
            group interaction by new { property.PropietarioId, interaction.Tipo } into ownerInteractions
            select new OwnerInteractionCount(ownerInteractions.Key.PropietarioId, ownerInteractions.Key.Tipo, ownerInteractions.Count()))
            .ToArrayAsync(ct);

        var visits = await (
            from visit in context.Visitas.AsNoTracking()
            join property in context.Inmuebles.AsNoTracking() on visit.InmuebleId equals property.Id
            where visit.Estado != "Cancelada" && visit.FechaProgramada >= start && visit.FechaProgramada < end
            group visit by property.PropietarioId into ownerVisits
            select new OwnerCount(ownerVisits.Key, ownerVisits.Count()))
            .ToDictionaryAsync(item => item.OwnerId, item => item.Count, ct);

        var receivedContacts = await (
            from lead in context.Leads.AsNoTracking()
            join publication in context.PropertyPublications.AsNoTracking() on lead.PublicationId equals publication.Id
            join property in context.Inmuebles.AsNoTracking() on publication.PropertyId equals property.Id
            where lead.FechaCreacion >= start && lead.FechaCreacion < end
            group lead by property.PropietarioId into ownerLeads
            select new OwnerCount(ownerLeads.Key, ownerLeads.Count()))
            .ToDictionaryAsync(item => item.OwnerId, item => item.Count, ct);

        var pageVisitCounts = pageVisits.ToDictionary(item => item.OwnerId, item => item.Count);
        var interactionCounts = interactions
            .GroupBy(item => item.OwnerId)
            .ToDictionary(group => group.Key, group => new
            {
                Calls = CountTypes(group, "llamada", "call"),
                Emails = CountTypes(group, "email", "correo"),
                Events = CountTypes(group, "evento", "event", "reunion", "reunión")
            });

        var rows = owners.Select(owner =>
        {
            var ownerInteractions = interactionCounts.GetValueOrDefault(owner.Id);
            return new OwnerPerformanceRow(
                owner.Id,
                owner.Nombre,
                pageVisitCounts.GetValueOrDefault(owner.Id),
                ownerInteractions?.Calls ?? 0,
                ownerInteractions?.Emails ?? 0,
                ownerInteractions?.Events ?? 0,
                visits.GetValueOrDefault(owner.Id),
                receivedContacts.GetValueOrDefault(owner.Id));
        }).ToArray();

        return new OwnerPerformanceReport(from, through, rows);
    }

    private static int CountTypes(IEnumerable<OwnerInteractionCount> interactions, params string[] types) =>
        interactions.Where(item => types.Contains(item.Type.Trim(), StringComparer.OrdinalIgnoreCase)).Sum(item => item.Count);

    private sealed record OwnerCount(Guid OwnerId, int Count);
    private sealed record OwnerInteractionCount(Guid OwnerId, string Type, int Count);
}
