using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PaymentOrderRepository(EnlyceDbContext context) : IPaymentOrderRepository
{
    public async Task AddAsync(PaymentOrder order, CancellationToken cancellationToken = default)
    {
        await context.PaymentOrders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<PaymentOrder?> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default) =>
        context.PaymentOrders.SingleOrDefaultAsync(
            order => order.Reference == reference,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
