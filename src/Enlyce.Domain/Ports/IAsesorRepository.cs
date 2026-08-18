using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IAsesorRepository
{
    Task<Asesor?> ObtenerPorCorreoAsync(string correo);
    Task<Asesor?> ObtenerPorIdAsync(Guid id);
    Task<bool> ExisteCorreoAsync(string correo);
    Task AgregarAsync(Asesor asesor);
    Task<List<Asesor>> ObtenerTodosAsync();
}
