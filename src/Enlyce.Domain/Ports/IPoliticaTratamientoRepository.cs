using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPoliticaTratamientoRepository
{
    Task<PoliticaTratamiento?> ObtenerActivaAsync();
    Task<PoliticaTratamiento?> ObtenerPorVersionAsync(string version);
    Task AgregarAsync(PoliticaTratamiento politica);
}
