using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Ports;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<Lead?> GetByEmailAsync(Email email);
    Task<IReadOnlyList<Lead>> GetAllAsync();
    Task<IReadOnlyList<Lead>> GetByAsesorIdAsync(Guid asesorId);
    Task<Lead> SaveAsync(Lead lead);
    Task<bool> ExistsByEmailAsync(Email email);
    Task<List<Lead>> ObtenerPorEtapaAsync(string etapa);
    Task<List<Lead>> ObtenerSinAsignarAsync();
    Task<int> ContarPorAsesorAsync(Guid asesorId);
    Task<int> ContarPorEtapaAsync(string etapa);
}
