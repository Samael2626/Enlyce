using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class AsesorTests
{
    private static Email ValidEmail() => Email.Create("asesor@test.com");
    private static string ValidHash() => BCrypt.Net.BCrypt.HashPassword("Password123!");

    [Fact]
    public void Crear_ConDatosValidos_CreaAsesor()
    {
        var asesor = Asesor.Crear("Juan Perez", ValidEmail(), ValidHash());
        Assert.Equal("Juan Perez", asesor.Nombre);
        Assert.Equal("asesor@test.com", asesor.Correo.Value);
        Assert.Equal("Asesor", asesor.Rol);
        Assert.True(asesor.Activo);
        Assert.NotEqual(Guid.Empty, asesor.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Crear_NombreVacio_LanzaDomainError(string? nombre)
    {
        Assert.Throws<DomainError>(() => Asesor.Crear(nombre!, ValidEmail(), ValidHash()));
    }

    [Theory]
    [InlineData("Gerente")]
    [InlineData("admin")]
    [InlineData("")]
    public void Crear_RolInvalido_LanzaDomainError(string rol)
    {
        Assert.Throws<DomainError>(() => Asesor.Crear("Juan", ValidEmail(), ValidHash(), rol));
    }

    [Fact]
    public void Crear_RolDefaultEsAsesor()
    {
        var asesor = Asesor.Crear("Juan", ValidEmail(), ValidHash());
        Assert.Equal("Asesor", asesor.Rol);
    }

    [Fact]
    public void VerificarPassword_PasswordCorrecto_RetornaTrue()
    {
        var password = "MiPassword123!";
        var asesor = Asesor.Crear("Juan", ValidEmail(), BCrypt.Net.BCrypt.HashPassword(password));
        Assert.True(asesor.VerificarPassword(password));
    }

    [Fact]
    public void VerificarPassword_PasswordIncorrecto_RetornaFalse()
    {
        var asesor = Asesor.Crear("Juan", ValidEmail(), ValidHash());
        Assert.False(asesor.VerificarPassword("WrongPassword!"));
    }

    [Fact]
    public void Reconstituir_CreaAsesorConTodosLosCampos()
    {
        var id = Guid.NewGuid();
        var fecha = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var correo = Email.Create("test@test.com");
        var hash = ValidHash();

        var asesor = Asesor.Reconstituir(id, "Test User", correo, hash, "Administrador", fecha, false);

        Assert.Equal(id, asesor.Id);
        Assert.Equal("Test User", asesor.Nombre);
        Assert.Equal("test@test.com", asesor.Correo.Value);
        Assert.Equal(hash, asesor.PasswordHash);
        Assert.Equal("Administrador", asesor.Rol);
        Assert.Equal(fecha, asesor.FechaCreacion);
        Assert.False(asesor.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Crear_PasswordHash_Vacio_LanzaDomainError(string? hash)
    {
        Assert.Throws<DomainError>(() => Asesor.Crear("Juan", ValidEmail(), hash!));
    }
}
