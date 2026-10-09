using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public sealed class CustomerDemandTests
{
    [Fact]
    public void Matches_RequiresAvailablePropertyWithinCustomerCriteria()
    {
        var demand = CustomerDemand.Create(Guid.NewGuid(), null, ModalidadInmueble.Venta,
            TipoInmueble.Apartamento, "Medellín", "Laureles", 300_000_000, 500_000_000,
            2, 2, 1, null);
        var matches = CreateProperty("Medellín", "Laureles", 400_000_000, EstadoInmueble.Disponible);
        var wrongBudget = CreateProperty("Medellín", "Laureles", 600_000_000, EstadoInmueble.Disponible);
        var unavailable = CreateProperty("Medellín", "Laureles", 400_000_000, EstadoInmueble.Vendido);

        Assert.True(demand.Matches(matches));
        Assert.False(demand.Matches(wrongBudget));
        Assert.False(demand.Matches(unavailable));
    }

    private static Inmueble CreateProperty(string city, string neighborhood, decimal price, EstadoInmueble status) =>
        Inmueble.Reconstituir(Guid.NewGuid(), "Apartamento", string.Empty, TipoInmueble.Apartamento,
            ModalidadInmueble.Venta, status, Direccion.Crear("Calle 1", city, neighborhood),
            Dinero.Crear(price), 80, 2, 2, 1, 0, Guid.NewGuid(), DateTime.UtcNow, null, true);
}
