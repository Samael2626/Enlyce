using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Direccion
{
    public string Calle { get; private set; }
    public string Ciudad { get; private set; }
    public string? Barrio { get; private set; }
    public string Pais { get; private set; } = "Colombia";
    public string? Departamento { get; private set; }

    private Direccion() { }
    private Direccion(string calle, string ciudad, string? barrio, string pais, string? departamento)
    {
        Calle = calle;
        Ciudad = ciudad;
        Barrio = barrio;
        Pais = pais;
        Departamento = departamento;
    }

    public static Direccion Crear(string calle, string ciudad, string? barrio = null,
        string pais = "Colombia", string? departamento = null)
    {
        if (string.IsNullOrWhiteSpace(calle))
            throw new DomainError("La direccion (calle) no puede ser vacia.");

        if (string.IsNullOrWhiteSpace(ciudad))
            throw new DomainError("La ciudad no puede ser vacia.");
        if (string.IsNullOrWhiteSpace(pais) || pais.Trim().Length > 100)
            throw new DomainError("El pais es obligatorio y no puede superar 100 caracteres.");
        if (departamento?.Trim().Length > 100)
            throw new DomainError("El departamento no puede superar 100 caracteres.");

        return new Direccion(calle.Trim(), ciudad.Trim(), barrio?.Trim(), pais.Trim(),
            string.IsNullOrWhiteSpace(departamento) ? null : departamento.Trim());
    }

    public override string ToString() =>
        Barrio is not null
            ? $"{Calle}, {Barrio}, {Ciudad}"
            : $"{Calle}, {Ciudad}";
}
