using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class PoliticaTratamiento
{
    public Guid Id { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string TextoCompleto { get; private set; } = string.Empty;
    public DateTime FechaVigencia { get; private set; }
    public bool Activa { get; private set; } = true;

    private PoliticaTratamiento() { }

    public static PoliticaTratamiento Crear(string version, string textoCompleto, DateTime fechaVigencia)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new DomainError("La version es obligatoria.");
        if (string.IsNullOrWhiteSpace(textoCompleto))
            throw new DomainError("El texto completo es obligatorio.");

        return new PoliticaTratamiento
        {
            Id = Guid.NewGuid(),
            Version = version,
            TextoCompleto = textoCompleto,
            FechaVigencia = fechaVigencia,
            Activa = true
        };
    }

    public static PoliticaTratamiento Reconstituir(
        Guid id, string version, string textoCompleto, DateTime fechaVigencia, bool activa)
    {
        return new PoliticaTratamiento
        {
            Id = id,
            Version = version,
            TextoCompleto = textoCompleto,
            FechaVigencia = fechaVigencia,
            Activa = activa
        };
    }

    public void Desactivar()
    {
        Activa = false;
    }
}
