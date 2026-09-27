using System.Text.RegularExpressions;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; private set; }

    private Email() { }
    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainError("Email no puede ser vacio.");

        var normalized = value.Trim();
        if (normalized.Length > 320)
            throw new DomainError("Email no puede superar 320 caracteres.");

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainError($"Email invalido: {value}");

        return new Email(normalized.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
