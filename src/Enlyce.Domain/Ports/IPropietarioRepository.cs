using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Ports;

public interface IPropietarioRepository
{
    Task<Propietario?> GetByIdAsync(Guid id);
    Task<Propietario?> GetByEmailAsync(Email email);
    Task<IReadOnlyList<Propietario>> GetAllAsync();
    Task<Propietario> SaveAsync(Propietario propietario);
    Task<bool> ExistsByEmailAsync(Email email);
}
