namespace Enlyce.Application.Publications;

public interface IPropertyPublicationAdminReadRepository
{
    Task<IReadOnlyList<PropertyPublicationAdminData>> GetAllAsync(
        Guid? advisorId, CancellationToken ct = default);

    Task<PropertyPublicationOptions> GetOptionsAsync(
        Guid? advisorId, CancellationToken ct = default);
}
