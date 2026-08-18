using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IInmuebleRepository
{
    Task<Inmueble?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Inmueble>> GetAllAsync();
    Task<IReadOnlyList<Inmueble>> GetByPropietarioIdAsync(Guid propietarioId);
    Task<Inmueble> SaveAsync(Inmueble inmueble);
}
