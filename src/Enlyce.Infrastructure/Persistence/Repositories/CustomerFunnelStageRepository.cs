using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class CustomerFunnelStageRepository(EnlyceDbContext context) : ICustomerFunnelStageRepository
{
    public async Task<IReadOnlyList<CustomerFunnelStage>> GetAllAsync(CancellationToken ct = default) =>
        await context.CustomerFunnelStages.AsNoTracking()
            .OrderBy(stage => stage.FunnelName).ThenBy(stage => stage.SortOrder).ThenBy(stage => stage.Name)
            .ToListAsync(ct);

    public Task<CustomerFunnelStage?> GetDefaultForInboundAsync(CancellationToken ct = default) =>
        context.CustomerFunnelStages.AsNoTracking()
            .SingleOrDefaultAsync(stage => stage.IsDefaultForInbound, ct);

    public Task<bool> IsStageAsync(string funnelName, string stageName, CancellationToken ct = default) =>
        context.CustomerFunnelStages.AsNoTracking().AnyAsync(stage =>
            stage.FunnelName == funnelName && stage.Name == stageName, ct);

    public async Task<bool> AddAsync(CustomerFunnelStage stage, CancellationToken ct = default)
    {
        if (await context.CustomerFunnelStages.AnyAsync(existing =>
            existing.FunnelName == stage.FunnelName && existing.Name == stage.Name, ct))
            return false;

        var maxOrder = await context.CustomerFunnelStages.Where(existing => existing.FunnelName == stage.FunnelName)
            .Select(existing => (int?)existing.SortOrder).MaxAsync(ct);
        if (maxOrder.HasValue && stage.SortOrder <= maxOrder.Value)
            stage.SetSortOrder(maxOrder.Value + 1);

        context.CustomerFunnelStages.Add(stage);
        try
        {
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(stage).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<bool> SetDefaultForInboundAsync(Guid stageId, CancellationToken ct = default)
    {
        var stage = await context.CustomerFunnelStages.SingleOrDefaultAsync(item => item.Id == stageId, ct);
        if (stage is null)
            return false;

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        await context.CustomerFunnelStages.Where(item => item.IsDefaultForInbound)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsDefaultForInbound, false), ct);
        stage.SetDefaultForInbound(true);
        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }
}
