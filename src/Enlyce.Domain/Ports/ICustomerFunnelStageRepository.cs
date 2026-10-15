using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ICustomerFunnelStageRepository
{
    Task<IReadOnlyList<CustomerFunnelStage>> GetAllAsync(CancellationToken ct = default);
    Task<CustomerFunnelStage?> GetDefaultForInboundAsync(CancellationToken ct = default);
    Task<bool> IsStageAsync(string funnelName, string stageName, CancellationToken ct = default);
    Task<bool> AddAsync(CustomerFunnelStage stage, CancellationToken ct = default);
    Task<bool> SetDefaultForInboundAsync(Guid stageId, CancellationToken ct = default);
}
