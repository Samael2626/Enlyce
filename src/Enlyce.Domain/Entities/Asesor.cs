using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public sealed class Asesor
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public Email Correo { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Rol { get; private set; } = "Asesor";
    public DateTime FechaCreacion { get; private set; }
    public bool Activo { get; private set; } = true;

    private Asesor() { }

    private Asesor(Guid id, string nombre, Email correo, string passwordHash,
        string rol, DateTime fechaCreacion, bool activo)
    {
        Id = id;
        Nombre = nombre;
        Correo = correo;
        PasswordHash = passwordHash;
        Rol = rol;
        FechaCreacion = fechaCreacion;
        Activo = activo;
    }

    public static Asesor Crear(string nombre, Email correo, string passwordHash, string rol = "Asesor")
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainError("El password hash es obligatorio.");

        if (rol != "Asesor" && rol != "Administrador")
            throw new DomainError("El rol debe ser Asesor o Administrador.");

        return new Asesor(
            Guid.NewGuid(),
            nombre.Trim(),
            correo,
            passwordHash,
            rol,
            DateTime.UtcNow,
            true);
    }

    public static Asesor Reconstituir(Guid id, string nombre, Email correo, string passwordHash,
        string rol, DateTime fechaCreacion, bool activo)
    {
        return new Asesor(id, nombre, correo, passwordHash, rol, fechaCreacion, activo);
    }

    public bool VerificarPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}
