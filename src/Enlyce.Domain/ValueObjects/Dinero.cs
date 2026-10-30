using System.Globalization;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Dinero
{
    // Formato colombiano fijo (punto de miles): no depende de la cultura del
    // servidor, que en contenedores Linux suele ser invariante.
    private static readonly NumberFormatInfo FormatoMiles = new() { NumberGroupSeparator = ".", NumberDecimalSeparator = "," };

    public decimal Monto { get; private set; }
    public string Moneda { get; private set; }

    private Dinero() { }
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

    public override string ToString() => $"{Moneda} {Monto.ToString("N0", FormatoMiles)}";
}
