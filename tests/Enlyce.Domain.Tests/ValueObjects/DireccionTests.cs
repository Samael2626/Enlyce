using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.ValueObjects;

public class DireccionTests
{
    [Fact]
    public void Create_Valid_ReturnsDireccion()
    {
        var dir = Direccion.Crear("Calle 10 #5-20", "Bogota", "Chapinero");
        Assert.Equal("Calle 10 #5-20", dir.Calle);
        Assert.Equal("Bogota", dir.Ciudad);
        Assert.Equal("Chapinero", dir.Barrio);
    }

    [Fact]
    public void Create_WithoutBarrio_BarrioIsNull()
    {
        var dir = Direccion.Crear("Calle 10 #5-20", "Bogota");
        Assert.Null(dir.Barrio);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_EmptyCalle_ThrowsDomainError(string? input)
    {
        Assert.Throws<DomainError>(() => Direccion.Crear(input!, "Bogota"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_EmptyCiudad_ThrowsDomainError(string? input)
    {
        Assert.Throws<DomainError>(() => Direccion.Crear("Calle 10", input!));
    }

    [Fact]
    public void ToString_WithBarrio_FormatsCorrectly()
    {
        var dir = Direccion.Crear("Calle 10", "Bogota", "Chapinero");
        Assert.Equal("Calle 10, Chapinero, Bogota", dir.ToString());
    }

    [Fact]
    public void ToString_WithoutBarrio_FormatsCorrectly()
    {
        var dir = Direccion.Crear("Calle 10", "Bogota");
        Assert.Equal("Calle 10, Bogota", dir.ToString());
    }
}
