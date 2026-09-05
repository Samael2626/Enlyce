namespace Enlyce.Application.PublicCatalog;

public interface IPublicPropertyReadRepository
{
    Task<PublicPropertyPageData> SearchAsync(
        PublicPropertySearchCriteria criteria,
        CancellationToken ct = default);

    Task<PublicPropertyDetailData?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default);
}
