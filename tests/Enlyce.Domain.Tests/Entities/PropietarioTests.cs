using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class PropietarioTests
{
    private static Email ValidEmail() => Email.Create("propietario@example.com");

    [Fact]
    public void Crear_Valido_ReturnsPropietario()
    {
        var prop = Propietario.Crear("Maria Lopez", ValidEmail());
        Assert.Equal("Maria Lopez", prop.Nombre);
        Assert.Equal("propietario@example.com", prop.Email.Value);
        Assert.True(prop.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Crear_NombreVacio_ThrowsDomainError(string? nombre)
    {
        Assert.Throws<DomainError>(() => Propietario.Crear(nombre!, ValidEmail()));
    }

    [Fact]
    public void Crear_ConTelefono_MantieneTelefono()
    {
        var phone = Telefono.Create("3101234567");
        var prop = Propietario.Crear("Maria", ValidEmail(), phone);
        Assert.NotNull(prop.Telefono);
        Assert.Equal("3101234567", prop.Telefono!.Value);
    }

    [Fact]
    public void Crear_SinTelefono_TelefonoEsNull()
    {
        var prop = Propietario.Crear("Maria", ValidEmail());
        Assert.Null(prop.Telefono);
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var prop = Propietario.Crear("Maria", ValidEmail());
        prop.Desactivar();
        Assert.False(prop.Activo);
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var fecha = DateTime.UtcNow;
        var prop = Propietario.Reconstituir(id, "Maria", ValidEmail(), null, fecha, true);
        Assert.Equal(id, prop.Id);
        Assert.Equal(fecha, prop.FechaCreacion);
    }
}
