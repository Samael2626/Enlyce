using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class AdvisorPerformanceReportReadRepository(EnlyceDbContext context)
    : IAdvisorPerformanceReportReadRepository
{
    public async Task<AdvisorActivityDetailReport?> GetDetailAsync(Guid advisorId, DateOnly since, DateOnly through, CancellationToken ct = default)
    {
        var exists = await context.Asesores.AsNoTracking()
            .AnyAsync(advisor => advisor.Id == advisorId && advisor.Rol == "Asesor", ct);
        if (!exists)
            return null;

        var start = since.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = through.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var interactions = await (
            from item in context.Interacciones.AsNoTracking()
            join lead in context.Leads.AsNoTracking() on item.LeadId equals lead.Id
            where item.AsesorId == advisorId && item.Fecha >= start && item.Fecha < end
            orderby item.Fecha descending
            select new AdvisorInteractionDetail(item.Fecha, item.Tipo, item.Resumen, lead.Nombre))
            .ToArrayAsync(ct);

        var visits = await (
            from item in context.Visitas.AsNoTracking()
            join lead in context.Leads.AsNoTracking() on item.LeadId equals lead.Id
            join property in context.Inmuebles.AsNoTracking() on item.InmuebleId equals property.Id
            where item.AsesorId == advisorId && item.Estado != "Cancelada" &&
                  item.FechaProgramada >= start && item.FechaProgramada < end
            orderby item.FechaProgramada descending
            select new AdvisorVisitDetail(item.FechaProgramada, item.FechaRealizada, item.Estado,
                item.Feedback, lead.Nombre, property.Nombre))
            .ToArrayAsync(ct);

        var deals = await (
            from item in context.RealizedDeals.AsNoTracking()
            join contact in context.Contacts.AsNoTracking() on item.ContactId equals contact.Id
            join property in context.Inmuebles.AsNoTracking() on item.PropertyId equals property.Id
            where item.AdvisorId == advisorId && item.ClosedOn >= since && item.ClosedOn <= through
            orderby item.ClosedOn descending
            select new { item.ClosedOn, item.Operation, PropertyName = property.Nombre,
                ClientName = contact.Name + (contact.Surname == null ? "" : " " + contact.Surname),
                item.GrossAmount, item.CommissionAmount, item.Currency })
            .ToArrayAsync(ct);

        return new AdvisorActivityDetailReport(advisorId, since, through, interactions, visits,
            deals.Select(item => new AdvisorDealDetail(item.ClosedOn, item.Operation.ToString(), item.PropertyName,
                item.ClientName, item.GrossAmount, item.CommissionAmount, item.Currency)).ToArray());
    }

    public async Task<AdvisorPerformanceReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default)
    {
        var length = through.DayNumber - from.DayNumber + 1;
        var previousThrough = from.AddDays(-1);
        var previousFrom = previousThrough.AddDays(-length + 1);
        var rangeStart = previousFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var currentStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var rangeEnd = through.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var advisors = await context.Asesores.AsNoTracking()
            .Where(advisor => advisor.Rol == "Asesor")
            .OrderBy(advisor => advisor.Nombre)
            .Select(advisor => new { advisor.Id, advisor.Nombre })
            .ToArrayAsync(ct);

        var interactions = await context.Interacciones.AsNoTracking()
            .Where(item => item.Fecha >= rangeStart && item.Fecha < rangeEnd)
            .GroupBy(item => new { item.AsesorId, IsCurrent = item.Fecha >= currentStart })
            .Select(group => new PeriodCount(group.Key.AsesorId, group.Key.IsCurrent, group.Count()))
            .ToArrayAsync(ct);

        var visits = await context.Visitas.AsNoTracking()
            .Where(item => item.Estado != "Cancelada" && item.FechaProgramada >= rangeStart && item.FechaProgramada < rangeEnd)
            .GroupBy(item => new { item.AsesorId, IsCurrent = item.FechaProgramada >= currentStart })
            .Select(group => new PeriodCount(group.Key.AsesorId, group.Key.IsCurrent, group.Count()))
            .ToArrayAsync(ct);

        var deals = await context.RealizedDeals.AsNoTracking()
            .Where(item => item.ClosedOn >= previousFrom && item.ClosedOn <= through)
            .GroupBy(item => new { item.AdvisorId, IsCurrent = item.ClosedOn >= from, item.Operation })
            .Select(group => new DealCount(group.Key.AdvisorId, group.Key.IsCurrent, group.Key.Operation, group.Count()))
            .ToArrayAsync(ct);

        var contacts = await context.Contacts.AsNoTracking()
            .Where(item => item.Active && item.OwnerAdvisorId != null &&
                           item.CreatedAt >= rangeStart && item.CreatedAt < rangeEnd)
            .GroupBy(item => new { AdvisorId = item.OwnerAdvisorId!.Value, IsCurrent = item.CreatedAt >= currentStart })
            .Select(group => new PeriodCount(group.Key.AdvisorId, group.Key.IsCurrent, group.Count()))
            .ToArrayAsync(ct);

        var properties = await context.Inmuebles.AsNoTracking()
            .Where(item => item.Activo && item.AsesorResponsableId != null &&
                           item.FechaCreacion >= rangeStart && item.FechaCreacion < rangeEnd)
            .GroupBy(item => new { AdvisorId = item.AsesorResponsableId!.Value, IsCurrent = item.FechaCreacion >= currentStart })
            .Select(group => new PeriodCount(group.Key.AdvisorId, group.Key.IsCurrent, group.Count()))
            .ToArrayAsync(ct);

        var interactionCounts = interactions.ToDictionary(item => (item.AdvisorId, item.IsCurrent), item => item.Count);
        var visitCounts = visits.ToDictionary(item => (item.AdvisorId, item.IsCurrent), item => item.Count);
        var dealCounts = deals.ToDictionary(item => (item.AdvisorId, item.IsCurrent, item.Operation), item => item.Count);
        var contactCounts = contacts.ToDictionary(item => (item.AdvisorId, item.IsCurrent), item => item.Count);
        var propertyCounts = properties.ToDictionary(item => (item.AdvisorId, item.IsCurrent), item => item.Count);

        var rows = advisors.Select(advisor => new AdvisorPerformanceRow(
            advisor.Id,
            advisor.Nombre,
            GetCount(interactionCounts, advisor.Id, true), GetCount(interactionCounts, advisor.Id, false),
            GetCount(visitCounts, advisor.Id, true), GetCount(visitCounts, advisor.Id, false),
            GetDealCount(dealCounts, advisor.Id, true, RealizedDealOperation.Sale),
            GetDealCount(dealCounts, advisor.Id, false, RealizedDealOperation.Sale),
            GetDealCount(dealCounts, advisor.Id, true, RealizedDealOperation.Rent),
            GetDealCount(dealCounts, advisor.Id, false, RealizedDealOperation.Rent),
            GetCount(contactCounts, advisor.Id, true), GetCount(contactCounts, advisor.Id, false),
            GetCount(propertyCounts, advisor.Id, true), GetCount(propertyCounts, advisor.Id, false)))
            .ToArray();

        return new AdvisorPerformanceReport(from, through, previousFrom, previousThrough, rows);
    }

    private static int GetCount(IReadOnlyDictionary<(Guid AdvisorId, bool IsCurrent), int> values, Guid advisorId, bool current) =>
        values.GetValueOrDefault((advisorId, current));

    private static int GetDealCount(IReadOnlyDictionary<(Guid AdvisorId, bool IsCurrent, RealizedDealOperation Operation), int> values,
        Guid advisorId, bool current, RealizedDealOperation operation) =>
        values.GetValueOrDefault((advisorId, current, operation));

    private sealed record PeriodCount(Guid AdvisorId, bool IsCurrent, int Count);
    private sealed record DealCount(Guid AdvisorId, bool IsCurrent, RealizedDealOperation Operation, int Count);
}
