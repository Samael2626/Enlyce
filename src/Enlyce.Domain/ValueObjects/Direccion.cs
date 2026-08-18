using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Direccion
{
    public string Calle { get; private set; }
    public string Ciudad { get; private set; }
    public string? Barrio { get; private set; }

    private Direccion() { }
    private Direccion(string calle, string ciudad, string? barrio)
    {
        Calle = calle;
        Ciudad = ciudad;
        Barrio = barrio;
    }

    public static Direccion Crear(string calle, string ciudad, string? barrio = null)
    {
        if (string.IsNullOrWhiteSpace(calle))
            throw new DomainError("La direccion (calle) no puede ser vacia.");

        if (string.IsNullOrWhiteSpace(ciudad))
            throw new DomainError("La ciudad no puede ser vacia.");

        return new Direccion(calle.Trim(), ciudad.Trim(), barrio?.Trim());
    }

    public override string ToString() =>
        Barrio is not null
            ? $"{Calle}, {Barrio}, {Ciudad}"
            : $"{Calle}, {Ciudad}";
}
