using Enlyce.Application.UseCases.ListInmuebles;
using NSubstitute;

namespace Enlyce.Application.Tests.UseCases;

public sealed class ListInmueblesHandlerTests
{
    [Fact]
    public async Task NormalizesPaginationAndPassesFilters()
    {
        var repository = Substitute.For<IInmuebleListReadRepository>();
        var expected = new ListInmueblesResponse([], 1, 100, 0);
        repository.ListAsync(Arg.Any<ListInmueblesQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
        var handler = new ListInmueblesHandler(repository);

        var result = await handler.HandleAsync(new ListInmueblesQuery(0, 500, "casa", "Venta", "Disponible"));

        Assert.Equal(expected, result);
        await repository.Received(1).ListAsync(
            Arg.Is<ListInmueblesQuery>(q => q.Page == 1 && q.PageSize == 100 && q.Search == "casa" && q.Modalidad == "Venta" && q.Estado == "Disponible"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmptyQueryReturnsEmptyPage()
    {
        var repository = Substitute.For<IInmuebleListReadRepository>();
        repository.ListAsync(Arg.Any<ListInmueblesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ListInmueblesResponse([], 1, 20, 0));

        var result = await new ListInmueblesHandler(repository).HandleAsync(new ListInmueblesQuery());

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }
}
