using Enlyce.Application.Abstractions;
using Enlyce.Application.Alerts;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using NSubstitute;

namespace Enlyce.Application.Tests.Alerts;

public sealed class PublicationAlertDispatcherTests
{
    private readonly IInmuebleRepository _properties = Substitute.For<IInmuebleRepository>();
    private readonly ICustomerDemandRepository _demands = Substitute.For<ICustomerDemandRepository>();
    private readonly IContactRepository _contacts = Substitute.For<IContactRepository>();
    private readonly IAsesorRepository _advisors = Substitute.For<IAsesorRepository>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IPublicLinks _links = Substitute.For<IPublicLinks>();

    private PublicationAlertDispatcher CreateSut() => new(_properties, _demands, _contacts, _advisors, _email, _links);

    [Fact]
    public async Task Dispatch_EmailsOnlyMatchingDemandsOnceAndNotifiesAdvisor()
    {
        var property = Property(400_000_000);
        var publication = PropertyPublication.Create(property.Id, Guid.NewGuid(), "apto-laureles", "Apto en Laureles", null);
        var contact = Contact.Create("Ana", Email.Create("ana@test.com"));
        var matching = Demand(contact.Id, 500_000_000);
        var tooCheap = Demand(Guid.NewGuid(), 100_000_000);
        var alreadyNotified = Demand(Guid.NewGuid(), 500_000_000);
        _properties.GetByIdAsync(property.Id).Returns(property);
        _demands.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([matching, tooCheap, alreadyNotified]);
        _demands.GetPropertyLinkAsync(alreadyNotified.Id, property.Id, Arg.Any<CancellationToken>())
            .Returns(DemandPropertyLink.Create(alreadyNotified.Id, property.Id));
        _contacts.GetByIdAsync(contact.Id, Arg.Any<CancellationToken>()).Returns(contact);
        _advisors.ObtenerPorIdAsync(publication.AdvisorId)
            .Returns(Asesor.Crear("Asesor", Email.Create("asesor@test.com"), "hash"));
        _links.PropertyUrl("apto-laureles").Returns("https://lyc.test/inmuebles/apto-laureles");

        var notified = await CreateSut().DispatchAsync(publication);

        Assert.Equal(1, notified);
        await _demands.Received(1).AddPropertyLinkAsync(
            Arg.Is<DemandPropertyLink>(link => link.DemandId == matching.Id), Arg.Any<CancellationToken>());
        await _email.Received(1).SendAsync("ana@test.com", Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("https://lyc.test/inmuebles/apto-laureles")));
        await _email.Received(1).SendAsync("asesor@test.com", Arg.Any<string>(), Arg.Is<string>(body => body.Contains("ana@test.com")));
    }

    [Fact]
    public async Task Dispatch_WithoutMatches_SendsNothing()
    {
        var property = Property(400_000_000);
        var publication = PropertyPublication.Create(property.Id, Guid.NewGuid(), "apto", "Apto", null);
        _properties.GetByIdAsync(property.Id).Returns(property);
        _demands.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([Demand(Guid.NewGuid(), 100_000_000)]);

        Assert.Equal(0, await CreateSut().DispatchAsync(publication));
        await _email.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!);
    }

    private static CustomerDemand Demand(Guid contactId, decimal maxPrice) =>
        CustomerDemand.Create(contactId, null, ModalidadInmueble.Venta, null, null, null, null, maxPrice, null, null, null, null);

    private static Inmueble Property(decimal price) =>
        Inmueble.Reconstituir(Guid.NewGuid(), "Apartamento", string.Empty, TipoInmueble.Apartamento,
            ModalidadInmueble.Venta, EstadoInmueble.Disponible, Direccion.Crear("Calle 1", "Medellín", "Laureles"),
            Dinero.Crear(price), 80, 2, 2, 1, 0, Guid.NewGuid(), DateTime.UtcNow, null, true);
}
