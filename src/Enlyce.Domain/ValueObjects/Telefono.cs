using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Telefono
{
    public string Value { get; private set; }

    private Telefono() { }
    private Telefono(string value) => Value = value;

    public static Telefono Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainError("Telefono no puede ser vacio.");

        var cleaned = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (cleaned.Length < 7 || cleaned.Length > 15)
            throw new DomainError($"Telefono invalido: {value}");

        if (!cleaned.All(char.IsDigit) && !cleaned.StartsWith("+"))
            throw new DomainError($"Telefono solo debe contener digitos o '+': {value}");

        return new Telefono(cleaned);
    }

    public override string ToString() => Value;
}
