using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class RealizedDealsReportReadRepository(EnlyceDbContext context) : IRealizedDealsReportReadRepository
{
    public async Task<RealizedDealsReportPage> GetAsync(RealizedDealsReportFilter filter, CancellationToken ct = default)
    {
        var query =
            from deal in context.RealizedDeals.AsNoTracking()
            join property in context.Inmuebles.AsNoTracking() on deal.PropertyId equals property.Id
            join contact in context.Contacts.AsNoTracking() on deal.ContactId equals contact.Id
            join advisor in context.Asesores.AsNoTracking() on deal.AdvisorId equals advisor.Id
            select new { deal, property, contact, advisor };

        if (filter.From.HasValue)
            query = query.Where(row => row.deal.ClosedOn >= filter.From.Value);
        if (filter.Through.HasValue)
            query = query.Where(row => row.deal.ClosedOn <= filter.Through.Value);
        if (filter.AdvisorId.HasValue)
            query = query.Where(row => row.deal.AdvisorId == filter.AdvisorId.Value);
        if (filter.Operation.HasValue)
            query = query.Where(row => row.deal.Operation == filter.Operation.Value);
        if (filter.PropertyType.HasValue)
            query = query.Where(row => row.property.Tipo == filter.PropertyType.Value);

        var totals = await query.GroupBy(row => row.deal.Currency)
            .Select(group => new RealizedDealsReportTotal(
                group.Key,
                group.Count(),
                group.Sum(row => row.deal.GrossAmount),
                group.Sum(row => row.deal.CommissionAmount)))
            .OrderBy(item => item.Currency)
            .ToArrayAsync(ct);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(row => row.deal.ClosedOn)
            .ThenByDescending(row => row.deal.RecordedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new RealizedDealReportItem(
                row.deal.Id,
                row.property.Nombre,
                row.property.Tipo.ToString(),
                row.deal.Operation.ToString(),
                row.deal.ClosedOn,
                row.advisor.Nombre,
                row.contact.Surname == null || row.contact.Surname == ""
                    ? row.contact.Name
                    : row.contact.Name + " " + row.contact.Surname,
                row.deal.GrossAmount,
                row.deal.CommissionAmount,
                row.deal.Currency,
                row.deal.AdvisorId,
                row.deal.RecordedAtUtc))
            .ToArrayAsync(ct);

        return new RealizedDealsReportPage(total, filter.Page, filter.PageSize, totals, items);
    }
}
