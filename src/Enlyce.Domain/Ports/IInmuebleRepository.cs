using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IInmuebleRepository
{
    Task<Inmueble?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Inmueble>> GetAllAsync();
    Task<IReadOnlyList<Inmueble>> GetByPropietarioIdAsync(Guid propietarioId);
    Task<IReadOnlyList<Inmueble>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Inmueble>> GetByIdsForUpdateAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Inmueble>> GetByExternalRefsAsync(IReadOnlyCollection<string> externalRefs);
    Task<Inmueble> SaveAsync(Inmueble inmueble);
}
