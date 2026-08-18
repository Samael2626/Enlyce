using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class InmuebleTests
{
    private static Direccion ValidDireccion() => Direccion.Crear("Calle 10 #5-20", "Bogota");
    private static Dinero ValidPrecio() => Dinero.Crear(500_000_000m);

    private static Inmueble CrearInmueble() => Inmueble.Crear(
        "Apto Chapinero", "3 habitaciones", TipoInmueble.Apartamento,
        ModalidadInmueble.Venta, ValidDireccion(), ValidPrecio(),
        80, 3, 2, 1, Guid.NewGuid());

    [Fact]
    public void Crear_Valido_ReturnsInmueble()
    {
        var inm = CrearInmueble();
        Assert.Equal("Apto Chapinero", inm.Nombre);
        Assert.Equal(TipoInmueble.Apartamento, inm.Tipo);
        Assert.Equal(EstadoInmueble.Disponible, inm.Estado);
        Assert.True(inm.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Crear_NombreVacio_ThrowsDomainError(string? nombre)
    {
        Assert.Throws<DomainError>(() => Inmueble.Crear(
            nombre!, "desc", TipoInmueble.Casa, ModalidadInmueble.Venta,
            ValidDireccion(), ValidPrecio(), 80, 3, 2, 1, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_PrecioCero_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Inmueble.Crear(
            "Apto", "desc", TipoInmueble.Casa, ModalidadInmueble.Venta,
            ValidDireccion(), Dinero.Crear(0m), 80, 3, 2, 1, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_MetrosCuadradosCero_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Inmueble.Crear(
            "Apto", "desc", TipoInmueble.Casa, ModalidadInmueble.Venta,
            ValidDireccion(), ValidPrecio(), 0, 3, 2, 1, Guid.NewGuid()));
    }

    [Fact]
    public void MarcarVendido_DesdeDisponible_CambiaAVendido()
    {
        var inm = CrearInmueble();
        inm.MarcarVendido();
        Assert.Equal(EstadoInmueble.Vendido, inm.Estado);
        Assert.NotNull(inm.FechaVentaArriendo);
    }

    [Fact]
    public void MarcarVendido_DesdeReservado_CambiaAVendido()
    {
        var propId = Guid.NewGuid();
        var inm = Inmueble.Reconstituir(Guid.NewGuid(), "Apto", "desc",
            TipoInmueble.Apartamento, ModalidadInmueble.Venta, EstadoInmueble.Reservado,
            ValidDireccion(), ValidPrecio(), 80, 3, 2, 1, 0, propId,
            DateTime.UtcNow, null, true);
        inm.MarcarVendido();
        Assert.Equal(EstadoInmueble.Vendido, inm.Estado);
    }

    [Fact]
    public void MarcarVendido_DesdeVendido_ThrowsDomainError()
    {
        var inm = CrearInmueble();
        inm.MarcarVendido();
        Assert.Throws<DomainError>(() => inm.MarcarVendido());
    }

    [Fact]
    public void MarcarArrendado_DesdeDisponible_CambiaAArrendado()
    {
        var inm = CrearInmueble();
        inm.MarcarArrendado();
        Assert.Equal(EstadoInmueble.Arrendado, inm.Estado);
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var inm = CrearInmueble();
        inm.Desactivar();
        Assert.False(inm.Activo);
    }
}
