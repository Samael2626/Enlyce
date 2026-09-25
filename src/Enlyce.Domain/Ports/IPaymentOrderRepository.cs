using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPaymentOrderRepository
{
    Task AddAsync(PaymentOrder order, CancellationToken cancellationToken = default);
    Task<PaymentOrder?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
