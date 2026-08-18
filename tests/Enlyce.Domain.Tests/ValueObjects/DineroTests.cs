using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.ValueObjects;

public class DineroTests
{
    [Fact]
    public void Create_ValidAmount_ReturnsDinero()
    {
        var dinero = Dinero.Crear(500_000m);
        Assert.Equal(500_000m, dinero.Monto);
        Assert.Equal("COP", dinero.Moneda);
    }

    [Fact]
    public void Create_ZeroAmount_ReturnsDinero()
    {
        var dinero = Dinero.Crear(0m);
        Assert.Equal(0m, dinero.Monto);
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Dinero.Crear(-1m));
    }

    [Fact]
    public void Create_EmptyMoneda_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Dinero.Crear(100m, ""));
    }

    [Fact]
    public void Create_MonedaNormalizesToUpper()
    {
        var dinero = Dinero.Crear(100m, "usd");
        Assert.Equal("USD", dinero.Moneda);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var dinero = Dinero.Crear(1_500_000m);
        Assert.Equal("COP 1.500.000", dinero.ToString());
    }
}
