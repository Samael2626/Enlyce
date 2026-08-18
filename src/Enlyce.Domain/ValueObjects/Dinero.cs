using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Dinero
{
    public decimal Monto { get; }
    public string Moneda { get; }

    private Dinero(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    public static Dinero Crear(decimal monto, string moneda = "COP")
    {
        if (monto < 0)
            throw new DomainError("El monto no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(moneda))
            throw new DomainError("La moneda no puede ser vacia.");

        return new Dinero(monto, moneda.ToUpperInvariant());
    }

    public override string ToString() => $"{Moneda} {Monto:N0}";
}
