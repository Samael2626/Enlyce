using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class InmuebleImportTests
{
    private static Inmueble CrearInmueble() => Inmueble.Crear(
        "Apto Chapinero", "3 habitaciones", TipoInmueble.Apartamento,
        ModalidadInmueble.Venta, Direccion.Crear("Calle 10 #5-20", "Bogota"),
        Dinero.Crear(500_000_000m), 80, 3, 2, 1, Guid.NewGuid());

    [Fact]
    public void Crear_SinOrigen_ExternalRefNulo()
    {
        Assert.Null(CrearInmueble().ExternalRef);
    }

    [Fact]
    public void VincularOrigenExterno_GuardaReferenciaRecortada()
    {
        var inm = CrearInmueble();
        inm.VincularOrigenExterno("  wasi:10472500:venta ");
        Assert.Equal("wasi:10472500:venta", inm.ExternalRef);
    }

    [Fact]
    public void VincularOrigenExterno_MismaReferencia_EsIdempotente()
    {
        var inm = CrearInmueble();
        inm.VincularOrigenExterno("wasi:1:venta");
        inm.VincularOrigenExterno("wasi:1:venta");
        Assert.Equal("wasi:1:venta", inm.ExternalRef);
    }

    [Fact]
    public void VincularOrigenExterno_OtraReferencia_ThrowsDomainError()
    {
        var inm = CrearInmueble();
        inm.VincularOrigenExterno("wasi:1:venta");
        Assert.Throws<DomainError>(() => inm.VincularOrigenExterno("wasi:2:venta"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VincularOrigenExterno_Vacia_ThrowsDomainError(string externalRef)
    {
        Assert.Throws<DomainError>(() => CrearInmueble().VincularOrigenExterno(externalRef));
    }

    [Fact]
    public void VincularOrigenExterno_MayorA100_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => CrearInmueble().VincularOrigenExterno(new string('x', 101)));
    }

    [Fact]
    public void ActualizarDatos_ReemplazaDescriptivos_SinTocarEstadoNiPropietario()
    {
        var inm = CrearInmueble();
        var propietario = inm.PropietarioId;
        inm.MarcarVendido();

        inm.ActualizarDatos("Finca San Vicente", "Nueva", TipoInmueble.Finca, ModalidadInmueble.Venta,
            Direccion.Crear("Vereda Las Frias", "San Vicente"), Dinero.Crear(3_500_000_000m), 336, 3, 2, 10);

        Assert.Equal("Finca San Vicente", inm.Nombre);
        Assert.Equal(TipoInmueble.Finca, inm.Tipo);
        Assert.Equal(336, inm.MetrosCuadrados);
        Assert.Equal(10, inm.Parqueaderos);
        Assert.Equal(EstadoInmueble.Vendido, inm.Estado);
        Assert.Equal(propietario, inm.PropietarioId);
    }

    [Fact]
    public void ActualizarDatos_PrecioCero_ThrowsDomainError()
    {
        var inm = CrearInmueble();
        Assert.Throws<DomainError>(() => inm.ActualizarDatos("Apto", "", TipoInmueble.Apartamento,
            ModalidadInmueble.Venta, Direccion.Crear("Calle 1", "Medellin"), Dinero.Crear(0m), 80, 1, 1, 0));
    }

    [Fact]
    public void Reconstituir_ConservaExternalRef()
    {
        var inm = Inmueble.Reconstituir(Guid.NewGuid(), "Apto", "desc", TipoInmueble.Apartamento,
            ModalidadInmueble.Arriendo, EstadoInmueble.Disponible, Direccion.Crear("Calle 1", "Medellin"),
            Dinero.Crear(3_000_000m), 42, 1, 1, 0, 0, Guid.NewGuid(), DateTime.UtcNow, null, true,
            "wasi:10443518:arriendo");
        Assert.Equal("wasi:10443518:arriendo", inm.ExternalRef);
    }
}
