using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public sealed class Propietario
{
    public Guid Id { get; }
    public string Nombre { get; }
    public Email Email { get; }
    public Telefono? Telefono { get; }
    public DateTime FechaCreacion { get; }
    public bool Activo { get; private set; }

    private Propietario(Guid id, string nombre, Email email, Telefono? telefono, DateTime fechaCreacion, bool activo)
    {
        Id = id;
        Nombre = nombre;
        Email = email;
        Telefono = telefono;
        FechaCreacion = fechaCreacion;
        Activo = activo;
    }

    public static Propietario Crear(string nombre, Email email, Telefono? telefono = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre del propietario no puede ser vacio.");

        return new Propietario(
            Guid.NewGuid(),
            nombre.Trim(),
            email,
            telefono,
            DateTime.UtcNow,
            true);
    }

    public static Propietario Reconstituir(Guid id, string nombre, Email email, Telefono? telefono,
        DateTime fechaCreacion, bool activo)
    {
        return new Propietario(id, nombre, email, telefono, fechaCreacion, activo);
    }

    public void Desactivar()
    {
        Activo = false;
    }
}
